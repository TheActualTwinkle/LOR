using System.Data.Common;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.WebApi;
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
        .WithImage("postgres:latest")
        .WithDatabase("lor")
        .WithUsername("postgres")
        .WithPassword("pass").Build();
    
    private Respawner _respawner = null!;
    private DbConnection _connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<IDatabaseContext, ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
            
            // Find ICacheService and Moq it. Every GetAsync should return null.
            var cacheServiceDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(ICacheService));

            if (cacheServiceDescriptor != null)
            {
                services.Remove(cacheServiceDescriptor);
            }
            
            services.AddScoped<ICacheService>(_ =>
            {
                Mock<ICacheService> mockCache = new();

                // TODO: Shouldn`t be string for <T?>, but Moq is bad with generics on return value.
                mockCache
                    .Setup(m => m.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((string _, CancellationToken _) => null);

                mockCache
                    .Setup(m => m.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                return mockCache.Object;
            });
        });
    }

    public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(_connection);

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _connection = Services.CreateScope().ServiceProvider.GetRequiredService<IDatabaseContext>()
            .Db.GetDbConnection();
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public new async Task DisposeAsync() =>
        await _dbContainer.DisposeAsync().AsTask();
}