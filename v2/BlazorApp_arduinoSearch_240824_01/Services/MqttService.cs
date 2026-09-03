using System.Text;
using BlazorApp_arduinoSearch_240824_01.Configuration;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace BlazorApp_arduinoSearch_240824_01.Services;

public sealed class MqttService : IDisposable
{
    private readonly MqttConnectionOptions _connectionOptions;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly IMqttClient _client;
    private MqttClientOptions? _lastClientOptions;
    private string _server = string.Empty;
    private int _port;
    private string _topic = string.Empty;
    private bool _disposed;
    private bool _disconnectRequested;

    public MqttService(IOptions<MqttConnectionOptions> connectionOptions)
    {
        _connectionOptions = connectionOptions.Value;

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _client.ConnectedAsync += async _ =>
        {
            await SubscribeCurrentTopic();
            SetStatus($"Connected to MQTT broker {_server}:{_port}.", clearError: true);
        };

        _client.DisconnectedAsync += async _ =>
        {
            if (_disposed || _disconnectRequested)
            {
                SetStatus("Disconnected from MQTT broker.", clearError: true);
                return;
            }

            SetStatus("Disconnected from MQTT broker. Reconnecting...");
            await TryReconnectAsync();
        };

        _client.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.PayloadSegment;
            var message = payload.Array == null
                ? string.Empty
                : Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);

            OnMessageReceived?.Invoke(topic, message);
            return Task.CompletedTask;
        };
    }

    public event Action<string, string>? OnMessageReceived;

    public event Action<string>? OnStatusChanged;

    public bool IsConnected => _client.IsConnected;

    public string LastStatus { get; private set; } = "Disconnected.";

    public string LastError { get; private set; } = string.Empty;

    public async Task<bool> ConnectAsync(
        string server,
        int port,
        string topic,
        CancellationToken cancellationToken = default)
    {
        server = server.Trim();
        topic = topic.Trim();

        if (string.IsNullOrWhiteSpace(server))
        {
            SetError("MQTT server address is required.");
            return false;
        }

        if (port is < 1 or > 65535)
        {
            SetError("MQTT port must be between 1 and 65535.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            SetError("MQTT topic is required.");
            return false;
        }

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_client.IsConnected && _server == server && _port == port && _topic == topic)
            {
                SetStatus($"Already connected to MQTT broker {server}:{port}.", clearError: true);
                return true;
            }

            if (_client.IsConnected)
            {
                _disconnectRequested = true;
                await _client.DisconnectAsync();
                _disconnectRequested = false;
            }

            _server = server;
            _port = port;
            _topic = topic;
            _lastClientOptions = new MqttClientOptionsBuilder()
                .WithClientId($"BlazorClient-{Guid.NewGuid():N}")
                .WithTcpServer(server, port)
                .Build();

            await _client.ConnectAsync(_lastClientOptions, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            SetError($"MQTT connection failed: {ex.Message}");
            return false;
        }
        finally
        {
            _disconnectRequested = false;
            _connectionLock.Release();
        }
    }

    public async Task<bool> PublishMessageAsync(
        string topic,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!_client.IsConnected)
        {
            SetError("MQTT broker is not connected.");
            return false;
        }

        topic = topic.Trim();

        if (string.IsNullOrWhiteSpace(topic))
        {
            SetError("MQTT publish topic is required.");
            return false;
        }

        try
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag(false)
                .Build();

            await _client.PublishAsync(mqttMessage, cancellationToken);
            SetStatus($"Published message to {topic}.", clearError: true);
            return true;
        }
        catch (Exception ex)
        {
            SetError($"MQTT publish failed: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _client.Dispose();
        _connectionLock.Dispose();
    }

    private async Task SubscribeCurrentTopic()
    {
        if (string.IsNullOrWhiteSpace(_topic))
        {
            return;
        }

        try
        {
            await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(_topic)
                .Build());
        }
        catch (Exception ex)
        {
            SetError($"MQTT subscribe failed: {ex.Message}");
        }
    }

    private async Task TryReconnectAsync()
    {
        if (_lastClientOptions == null)
        {
            SetError("MQTT reconnect options are not available.");
            return;
        }

        for (var attempt = 1; attempt <= Math.Max(1, _connectionOptions.MaxReconnectAttempts); attempt++)
        {
            try
            {
                await Task.Delay(Math.Max(0, _connectionOptions.ReconnectDelayMilliseconds));
                await _client.ConnectAsync(_lastClientOptions);
                SetStatus($"Reconnected to MQTT broker {_server}:{_port}.", clearError: true);
                return;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                SetStatus($"MQTT reconnect attempt {attempt} failed.");
            }
        }

        SetError("MQTT reconnect attempts were exhausted.");
    }

    private void SetStatus(string status, bool clearError = false)
    {
        if (clearError)
        {
            LastError = string.Empty;
        }

        LastStatus = status;
        OnStatusChanged?.Invoke(status);
    }

    private void SetError(string error)
    {
        LastError = error;
        SetStatus(error);
    }
}
