using System.Text.Json;
using MudBlazorWebApp240916.Services;
using MudBlazorWebApp240916.Shared.DataModel;

namespace MudBlazorWebApp240916;

public static class IoTApi
{
    public static IEndpointRouteBuilder MapIoTApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/iot");

        api.MapGet("/status", (MqttGateway gateway) => Results.Ok(gateway.GetStatus()));

        api.MapPost("/connect", async (MqttConnectionRequest request, MqttGateway gateway, CancellationToken token) =>
        {
            try
            {
                await gateway.ConnectAsync(request, token);
                return Results.Ok(new ApiResult { Success = true, Message = "MQTT 브로커에 연결했습니다." });
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return Results.BadRequest(new ApiResult { Success = false, Message = exception.Message });
            }
        });

        api.MapPost("/disconnect", async (MqttGateway gateway, CancellationToken token) =>
        {
            await gateway.DisconnectAsync(token);
            return Results.Ok(new ApiResult { Success = true, Message = "MQTT 연결을 종료했습니다." });
        });

        api.MapPost("/publish", async (MqttPublishRequest request, MqttGateway gateway, CancellationToken token) =>
        {
            try
            {
                await gateway.PublishAsync(request, token);
                return Results.Ok(new ApiResult { Success = true, Message = "메시지를 발행했습니다." });
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return Results.BadRequest(new ApiResult { Success = false, Message = exception.Message });
            }
        });

        api.MapPost("/unity", async (MqttPublishRequest request, MqttGateway gateway, CancellationToken token) =>
        {
            gateway.AcceptUnityTelemetry(request);
            if (gateway.GetStatus().IsConnected)
            {
                await gateway.PublishAsync(request, token);
            }
            return Results.Ok(new ApiResult { Success = true, Message = "Unity 데이터를 브리지에 반영했습니다." });
        });

        api.MapGet("/modules", (DeviceModuleRegistry registry) => Results.Ok(registry.GetAll()));
        api.MapPost("/modules", (DeviceModule module, DeviceModuleRegistry registry) =>
        {
            if (string.IsNullOrWhiteSpace(module.Name) || string.IsNullOrWhiteSpace(module.MqttHost) ||
                string.IsNullOrWhiteSpace(module.TelemetryTopic) || module.MqttPort is < 1 or > 65535)
            {
                return Results.BadRequest(new ApiResult { Success = false, Message = "이름, MQTT 호스트/포트, 텔레메트리 토픽을 확인하세요." });
            }
            return Results.Ok(registry.Add(module));
        });
        api.MapDelete("/modules/{id:guid}", (Guid id, DeviceModuleRegistry registry) =>
            registry.Remove(id) ? Results.NoContent() : Results.NotFound());

        api.MapGet("/events", StreamEventsAsync);
        return endpoints;
    }

    private static async Task StreamEventsAsync(HttpContext context, TelemetryStreamBroker broker)
    {
        context.Response.Headers.CacheControl = "no-cache, no-store";
        context.Response.Headers.Append("X-Accel-Buffering", "no");
        context.Response.ContentType = "text/event-stream";
        var subscription = broker.Subscribe();
        try
        {
            await context.Response.WriteAsync(": connected\n\n", context.RequestAborted);
            await context.Response.Body.FlushAsync(context.RequestAborted);
            await foreach (var message in subscription.Reader.ReadAllAsync(context.RequestAborted))
            {
                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                await context.Response.WriteAsync($"event: telemetry\ndata: {json}\n\n", context.RequestAborted);
                await context.Response.Body.FlushAsync(context.RequestAborted);
            }
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Browser navigation closes the stream normally.
        }
        finally
        {
            broker.Unsubscribe(subscription.Id);
        }
    }
}
