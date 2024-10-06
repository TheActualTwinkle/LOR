using System.Data.Common;
using DatabaseApp.WebApi;
using Grpc.Net.Client;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace DatabaseApp.Tests.TestContext;

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
        .WithPortBinding(5672)
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();
    
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .WithPortBinding(6379)
        .Build();
    
    private Respawner _respawner = null!;
    private DbConnection _connection = null!;
    
    public async Task StartAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
        await _redisContainer.StartAsync();

        const string hostUrl = "http://localhost:31401";
#pragma warning disable CS4014
        Program.Main(["--urls", hostUrl]);
#pragma warning restore CS4014
        
        Console.WriteLine("Waiting for the database to start...");

        // Ensure that the DatabaseApp is started and configured
        await GrpcChannel.ForAddress(hostUrl).ConnectAsync();

        Console.WriteLine("Database started. Initializing respawner...");

        _connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _connection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task StopAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        
        await Task.WhenAll(
            _dbContainer.DisposeAsync().AsTask(),
            _rabbitMqContainer.DisposeAsync().AsTask(),
            _redisContainer.DisposeAsync().AsTask());
    }

    public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(_connection);
}