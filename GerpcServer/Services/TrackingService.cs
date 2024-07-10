﻿using GerpcServer.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GerpcServer.Services;

public class TelemetryTrackingService : TrackingService.TrackingServiceBase
{
    private readonly ILogger<TelemetryTrackingService> logger;

    public TelemetryTrackingService(ILogger<TelemetryTrackingService> logger)
    {
        this.logger = logger;
    }

    public override Task<TrackingResponse> SendMessage(TrackingMessage request, ServerCallContext context)
    {
        logger.LogInformation($"New Message: DeviceId: {request.DeviceId} " +
            $"Location: ({request.Location.Lat},{request.Location.Long}) Speed: {request.Speed} Sensors: {request.Sensors.Count}");

        return Task.FromResult(new TrackingResponse { Success = true });
    }

    public override async Task<Empty> KeepAlive(IAsyncStreamReader<PulseMessage> requestStream, ServerCallContext context)
    {
        var task = Task.Run(async () =>
        {
            await foreach (var item in requestStream.ReadAllAsync())
            {
                logger.LogInformation($"{nameof(KeepAlive)}: Status: {item.Status} Details: {item.Details}");
            }
        });

        await task;

        return new Empty();
    }
}
