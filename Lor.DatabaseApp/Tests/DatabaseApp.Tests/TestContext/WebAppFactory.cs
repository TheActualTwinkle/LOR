using System.Data.Common;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Messaging.Consumers;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.WebApi;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace DatabaseApp.Tests.TestContext;

public class WebAppFactory : WebApplicationFactory<Program>
{
    private const string RabbitMqUsername = "guest";
    private const string RabbitMqPassword = "guest";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("lor")
        .WithUsername("postgres")
        .WithPassword("123456789")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:latest")
        .WithUsername(RabbitMqUsername)
        .WithPassword(RabbitMqPassword)
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _connection = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        _connection = Services.CreateScope()
            .ServiceProvider.GetRequiredService<IDatabaseContext>()
            .Db.GetDbConnection();

        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            _connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"] // TODO: https://github.com/TheActualTwinkle/LOR/issues/59
            });
    }

    public async Task ResetDatabaseAsync() =>
        await _respawner.ResetAsync(_connection);

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync().AsTask();
        await _rabbitMqContainer.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            ConfigureDatabase(services);

            ConfigureMassTransit(services);

            MockCache(services);
        });

    private void ConfigureMassTransit(IServiceCollection services)
    {
        var massTransitDescriptors = services.Where(d =>
                d.ServiceType.Namespace?.Contains("MassTransit") == true ||
                d.ImplementationType?.Namespace?.Contains("MassTransit") == true)
            .ToList();
        
        foreach (var descriptor in massTransitDescriptors)
            services.Remove(descriptor);

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumer<NewClassesReminderConsumer>();
            x.AddConsumer<NewClassesRemovalConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(_rabbitMqContainer.GetConnectionString());

                cfg.ReceiveEndpoint(
                    "dba-new-classes-reminder",
                    e => e.Consumer<NewClassesReminderConsumer>(context));

                cfg.ReceiveEndpoint(
                    "dba-new-classes-removal",
                    e => e.Consumer<NewClassesRemovalConsumer>(context));
            });
        });
    }

    private void ConfigureDatabase(IServiceCollection services)
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
    }

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
}