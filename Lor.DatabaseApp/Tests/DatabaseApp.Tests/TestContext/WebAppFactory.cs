using System.Data.Common;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            ServiceDescriptor? descriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<IDatabaseContext, ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
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