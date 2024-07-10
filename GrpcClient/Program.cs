using GrpcClient;

var builder = Host.CreateApplicationBuilder(args);

Console.Write("Enter Device ID:");
var deviceId = Int32.Parse(Console.ReadLine());

builder.Services.AddHostedService<TrackingWorker>(provider =>
{
    var logger = provider.GetService<ILogger<TrackingWorker>>();
    return  new TrackingWorker(logger, deviceId);
});



var host = builder.Build();
host.Run();
