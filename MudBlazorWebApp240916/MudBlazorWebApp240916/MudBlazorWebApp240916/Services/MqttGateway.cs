using System.Globalization;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MudBlazorWebApp240916.Shared.DataModel;

namespace MudBlazorWebApp240916.Services;

public sealed class MqttGateway : IAsyncDisposable
{
    private const int MaximumPayloadBytes = 256 * 1024;
    private readonly IMqttClient _client;
    private readonly TelemetryStreamBroker _streamBroker;
    private readonly ILogger<MqttGateway> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly object _statusLock = new();
    private MqttGatewayStatus _status = new();

    public MqttGateway(TelemetryStreamBroker streamBroker, ILogger<MqttGateway> logger)
    {
        _streamBroker = streamBroker;
        _logger = logger;
        _client = new MqttFactory().CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _client.DisconnectedAsync += args =>
        {
            lock (_statusLock)
            {
                _status.IsConnected = false;
                _status.LastError = args.Exception?.Message;
            }
            return Task.CompletedTask;
        };
    }

    public MqttGatewayStatus GetStatus()
    {
        lock (_statusLock)
        {
            return new MqttGatewayStatus
            {
                IsConnected = _client.IsConnected,
                Host = _status.Host,
                Port = _status.Port,
                Topics = [.. _status.Topics],
                LastError = _status.LastError,
                ConnectedAt = _status.ConnectedAt,
                ReceivedMessages = _status.ReceivedMessages
            };
        }
    }

    public async Task ConnectAsync(MqttConnectionRequest request, CancellationToken cancellationToken)
    {
        ValidateConnection(request);
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync(cancellationToken: cancellationToken);
            }

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId($"smart-grow-{Guid.NewGuid():N}")
                .WithTcpServer(request.Host.Trim(), request.Port)
                .WithCleanSession();

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                optionsBuilder.WithCredentials(request.Username, request.Password);
            }

            await _client.ConnectAsync(optionsBuilder.Build(), cancellationToken);

            foreach (var topic in request.Topics.Select(topic => topic.Trim()).Distinct(StringComparer.Ordinal))
            {
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(filter => filter
                        .WithTopic(topic)
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .Build();
                await _client.SubscribeAsync(subscribeOptions, cancellationToken);
            }

            lock (_statusLock)
            {
                _status = new MqttGatewayStatus
                {
                    IsConnected = true,
                    Host = request.Host.Trim(),
                    Port = request.Port,
                    Topics = request.Topics.Select(topic => topic.Trim()).Distinct().ToList(),
                    ConnectedAt = DateTimeOffset.UtcNow
                };
            }
        }
        catch (Exception exception)
        {
            lock (_statusLock)
            {
                _status.IsConnected = false;
                _status.LastError = exception.Message;
            }
            _logger.LogWarning(exception, "MQTT broker connection failed for {Host}:{Port}", request.Host, request.Port);
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync(cancellationToken: cancellationToken);
            }
            lock (_statusLock)
            {
                _status.IsConnected = false;
                _status.LastError = null;
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task PublishAsync(MqttPublishRequest request, CancellationToken cancellationToken)
    {
        if (!_client.IsConnected)
        {
            throw new InvalidOperationException("MQTT 브로커가 연결되지 않았습니다.");
        }
        if (string.IsNullOrWhiteSpace(request.Topic) || request.Topic.IndexOfAny(['#', '+']) >= 0)
        {
            throw new ArgumentException("발행 토픽은 비어 있거나 와일드카드를 포함할 수 없습니다.");
        }
        if (System.Text.Encoding.UTF8.GetByteCount(request.Payload ?? string.Empty) > MaximumPayloadBytes)
        {
            throw new ArgumentException($"MQTT payload는 {MaximumPayloadBytes / 1024}KB 이하여야 합니다.");
        }

        var qos = request.Qos switch
        {
            1 => MqttQualityOfServiceLevel.AtLeastOnce,
            2 => MqttQualityOfServiceLevel.ExactlyOnce,
            _ => MqttQualityOfServiceLevel.AtMostOnce
        };
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(request.Topic.Trim())
            .WithPayload(request.Payload ?? string.Empty)
            .WithQualityOfServiceLevel(qos)
            .WithRetainFlag(request.Retain)
            .Build();
        await _client.PublishAsync(message, cancellationToken);
    }

    public void AcceptUnityTelemetry(MqttPublishRequest request)
    {
        var envelope = CreateEnvelope(request.Topic, request.Payload, "unity");
        _streamBroker.Publish(envelope);
    }

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var envelope = CreateEnvelope(
            args.ApplicationMessage.Topic,
            args.ApplicationMessage.ConvertPayloadToString(),
            "mqtt");
        lock (_statusLock)
        {
            _status.ReceivedMessages++;
        }
        _streamBroker.Publish(envelope);
        return Task.CompletedTask;
    }

    internal static TelemetryEnvelope CreateEnvelope(string topic, string? payload, string source)
    {
        var envelope = new TelemetryEnvelope
        {
            Topic = topic ?? string.Empty,
            Payload = payload ?? string.Empty,
            Source = source,
            ReceivedAt = DateTimeOffset.UtcNow
        };

        if (double.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var scalar))
        {
            envelope.Values[MetricNameFromTopic(topic)] = scalar;
            return envelope;
        }

        try
        {
            using var document = JsonDocument.Parse(payload ?? string.Empty);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetDouble(out var value))
                    {
                        envelope.Values[property.Name] = value;
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Text commands are valid MQTT payloads; they simply have no chart values.
        }

        return envelope;
    }

    private static string MetricNameFromTopic(string? topic)
    {
        var leaf = topic?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return string.IsNullOrWhiteSpace(leaf) ? "value" : leaf;
    }

    private static void ValidateConnection(MqttConnectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Host))
        {
            throw new ArgumentException("MQTT 호스트를 입력하세요.");
        }
        if (request.Port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Port), "MQTT 포트는 1~65535 범위여야 합니다.");
        }
        if (request.Topics.Count == 0 || request.Topics.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("구독 토픽을 하나 이상 입력하세요.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
        }
        _client.Dispose();
        _connectionLock.Dispose();
    }
}
