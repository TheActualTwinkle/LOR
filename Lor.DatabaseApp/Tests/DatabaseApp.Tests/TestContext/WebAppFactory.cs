using System.Data.Common;
using System.Linq.Expressions;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.WebApi;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Respawn;
using Testcontainers.PostgreSql;

namespace DatabaseApp.Tests.TestContext;

public class WebAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("lor")
        .WithUsername("postgres")
        .WithPassword("123456789")
        .Build();
    
    private Respawner _respawner = null!;
    private DbConnection _connection = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        _connection = Services.CreateScope()
            .ServiceProvider.GetRequiredService<IDatabaseContext>()
            .Db.GetDbConnection();
        
        await _connection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public", "hangfire"]
        });
    }

    public async Task ResetDatabaseAsync() =>
        await _respawner.ResetAsync(_connection);

    public new async Task DisposeAsync() =>
        await _dbContainer.DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var connectionString = _dbContainer.GetConnectionString();
            
            services.AddDbContext<IDatabaseContext, ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            
            services.AddHangfire(c =>
                c.UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(connectionString))
                    .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));
            
            MockCache(services);
        });
    }

    #region Mocks

    private static void MockCache(IServiceCollection services)
    {
        var cacheServiceDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ICacheService));

        if (cacheServiceDescriptor != null)
            services.Remove(cacheServiceDescriptor);

        services.AddScoped<ICacheService>(_ =>
        {
            Mock<ICacheService> mock = new();

            mock
                .Setup(m => m.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string _, CancellationToken _) => null);

            mock
                .Setup(m => m.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mock.Object;
        });
    }
    
    #endregion
}