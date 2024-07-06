using GerpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGrpcService<TelemetryTrackingService>();

app.Run();
