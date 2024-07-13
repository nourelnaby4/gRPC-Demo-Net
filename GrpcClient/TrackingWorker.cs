using GerpcServer.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

namespace GrpcClient;

public class TrackingWorker : BackgroundService
{
    private readonly ILogger<TrackingWorker> _logger;
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

    public TrackingWorker(ILogger<TrackingWorker> logger, int deviceId)
    {
        _logger = logger;
        _deviceId = deviceId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Random random = new Random();

        var keepAliveTask = KeepAlive(stoppingToken);

        var subsribeTask = SubscribeNotification(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SendMessage(random);

            await Task.Delay(1000, stoppingToken);
        }

        await Task.WhenAll(keepAliveTask , subsribeTask);
    }

    private async Task SubscribeNotification(CancellationToken stoppingToken)
    {
        var responseStream = Client.SubscribeNotification(new SubsrcribeRequest { DeviceId = _deviceId });

        var task = Task.Run(async () =>
        {
            while (await responseStream.ResponseStream.MoveNext(stoppingToken))
            {
                var msg = responseStream.ResponseStream.Current;

                _logger.LogInformation($"new message recieved from {msg.Text}");
            }
        });

        await task;
    }

    private async Task KeepAlive(CancellationToken stoppingToken)
    {
        var stream = Client.KeepAlive();

        var keepAliveTask = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await stream.RequestStream.WriteAsync(new PulseMessage
                {
                    Status = ClientStatus.Working,
                    Details = $"Device Id: {_deviceId} is working!",
                    Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
                });

                await Task.Delay(2000);
            }
        });

        await keepAliveTask;
    }

    private async Task SendMessage(Random random)
    {
        var request = new TrackingMessage
        {
            DeviceId = _deviceId,
            Speed = random.Next(0, 220),
            Location = new Location { Lat = random.Next(0, 100), Long = random.Next(0, 100) },
            Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        request.Sensors.Add(new Sensor { Key = "temp", Value = 30 });
        request.Sensors.Add(new Sensor { Key = "door", Value = 1 });

        var response = await Client.SendMessageAsync(request);

        _logger.LogInformation($"Response: {response.Success}");
    }
}
