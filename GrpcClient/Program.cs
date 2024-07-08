using GrpcClient;

var builder = Host.CreateApplicationBuilder(args);

Console.WriteLine("Enter Device ID:");
var deviceId = Int32.Parse(Console.ReadLine());

builder.Services.AddHostedService<Worker>(provider =>
{
    var logger = provider.GetService<ILogger<Worker>>();
    return  new Worker(logger, deviceId);
});

var host = builder.Build();
host.Run();
