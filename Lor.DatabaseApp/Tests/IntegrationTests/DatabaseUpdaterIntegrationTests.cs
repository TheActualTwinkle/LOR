using System.Diagnostics;
using GroupScheduleApp.AppCommunication.Grpc;
using Grpc.Net.Client;

namespace IntegrationTests;

public class DatabaseUpdaterIntegrationTests
{
    private GrpcDatabaseUpdaterClient _client;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        // Start Docker Compose
        ProcessStartInfo startInfo = new()
        {
            FileName = "docker-compose",
            Arguments = "up -d",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using (Process? process = Process.Start(startInfo))
        {
            await process.WaitForExitAsync();
        }

        // Create gRPC channel and client
        _client = new GrpcDatabaseUpdaterClient("http://localhost:50051");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Stop Docker Compose
        ProcessStartInfo stopInfo = new()
        {
            FileName = "docker-compose",
            Arguments = "down",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using Process? process = Process.Start(stopInfo);
        await process!.WaitForExitAsync();
    }

    [Test]
    public async Task TestGrpcMethod()
    {
        var response = await _client.SetAvailableGroups(new []{});
        Assert.IsNotNull(response);
        // Additional assertions
    }
}