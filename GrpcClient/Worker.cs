using GerpcServer.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

namespace GrpcClient;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly int _deviceId;
    private TrackingService.TrackingServiceClient _client;

    private TrackingService.TrackingServiceClient Client
    {
        get
        {
            if (_client is null)
            {
                var channel = GrpcChannel.ForAddress("http://localhost:5240");
                _client = new TrackingService.TrackingServiceClient(channel);
            }
            return _client;
        }
    }

    public Worker(ILogger<Worker> logger, int deviceId)
    {
        _logger = logger;
        _deviceId = deviceId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Random random = new Random();
        while (!stoppingToken.IsCancellationRequested)
        {
            var requset = new TrackingMessage
            {
                DeviceId = _deviceId,
                Speed = random.Next(0, 220),
                Location = new Location { Lat = random.Next(0, 100), Long = random.Next(50, 200) },
                Stamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Sensor = new Sensor { Key = "temp", Value = 30 }
            };

            var response = await Client.SendMessageAsync(requset);

            _logger.LogInformation($"Response: {response.Success}");

            await Task.Delay(1000, stoppingToken);
        }
    }
}
