using System.Net;
using DatabaseApp.AppCommunication;
using DatabaseApp.AppCommunication.Grpc;
using DatabaseApp.Application;
using DatabaseApp.Application.User.Command.ConfirmUserEmail;
using DatabaseApp.Caching;
using DatabaseApp.Persistence;
using DatabaseApp.Persistence.DatabaseContext;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

// const string httpsPortKey = "HttpsPort"; 

// DB где-то тут.

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables();

builder.Services.AddGrpc();
builder.Services.AddApplication();
builder.Services.AddCaching(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddBus(builder.Configuration);

// builder.Services.AddHttpsRedirection(options =>
// {
//     options.HttpsPort = builder.Configuration.GetValue<int>(httpsPortKey);
// });
//
// builder.Services.AddHsts(options =>
// {
//     options.Preload = true;
//     options.IncludeSubDomains = true;
// });

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(31401, o => o.Protocols = HttpProtocols.Http2);
    options.Listen(IPAddress.Any, 31402, o => o.Protocols = HttpProtocols.Http1);
});


WebApplication app = builder.Build();

try
{
    using IServiceScope scope = app.Services.CreateScope();
    IDatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<IDatabaseContext>();
    await databaseContext.Db.MigrateAsync();
}
catch (Exception e)
{
    Console.WriteLine($"Error: {e.Message}");
}

// TODO
// app.UseHsts();
// app.UseHttpsRedirection();
app.MapGrpcService<GrpcDatabaseService>();
app.MapGrpcService<GrpcDatabaseUpdaterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.MapGet("/email-verification", async (Guid token, ISender mediator) =>
{
    var result = await mediator.Send(new ConfirmUserEmailCommand { TokenIdentifier = token });
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Errors);
});

app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;