using System.Data.Common;
using DatabaseApp.Persistence.DatabaseContext;
using DatabaseApp.WebApi;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace TelegramBotApp.Tests.TestContext;

public class DatabaseAppFactory
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithPortBinding(5432)
        .WithDatabase("lor")
        .WithUsername("postgres")
        .WithPassword("123456789")
        .Build();
    
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:latest")
        .WithHostname("rabbitmq")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();
    
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .WithPortBinding(6379)
        .Build();
    
    private Respawner _respawner = null!;
    private DbConnection _connection = null!;

    public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(_connection);

    public async Task StartAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        Console.WriteLine(_dbContainer.GetConnectionString());
        
#pragma warning disable CS4014
        Program.Main(["--urls", "http://localhost:31401"]);
#pragma warning restore CS4014 
        
        _connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task StopAsync() =>
        await Task.WhenAll(
            _dbContainer.DisposeAsync().AsTask(),
            _rabbitMqContainer.DisposeAsync().AsTask());
}