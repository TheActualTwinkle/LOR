using GroupScheduleApp.Grpc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GroupScheduleService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client.");

await app.RunAsync();