using GerpcServer.Protos;
using Grpc.Core;
using System.Text;

namespace GerpcServer.Services;

public class TelemetryTrackingService : TrackingService.TrackingServiceBase
{
    private readonly ILogger<TelemetryTrackingService> _logger;
    public TelemetryTrackingService(ILogger<TelemetryTrackingService> logger)
    {
        _logger = logger;
    }
    public override Task<TrackingResponse> SendMessage(TrackingMessage request, ServerCallContext context)
    {
        var information = new StringBuilder();

        information.AppendLine($"deviceId {request.DeviceId}: {request.DeviceId}");
        information.AppendLine($"location lat{request.Location.Lat}, long:{request.Location.Long}");
        information.AppendLine($"speed {request.Speed}");
        information.AppendLine($"datetime {request.Stamp}");

        _logger.LogInformation(information.ToString());

        return Task.FromResult(new TrackingResponse { Success = true });
    }
}
