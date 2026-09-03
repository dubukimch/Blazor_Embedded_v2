namespace MudBlazorWebApp240916.Shared.DataModel;

public sealed class MqttConnectionRequest
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public List<string> Topics { get; set; } = ["farm/+/telemetry"];
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class MqttPublishRequest
{
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int Qos { get; set; }
    public bool Retain { get; set; }
    public string Source { get; set; } = "blazor";
}

public sealed class MqttGatewayStatus
{
    public bool IsConnected { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public List<string> Topics { get; set; } = [];
    public string? LastError { get; set; }
    public DateTimeOffset? ConnectedAt { get; set; }
    public long ReceivedMessages { get; set; }
}

public sealed class TelemetryEnvelope
{
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Source { get; set; } = "mqtt";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, double> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class DeviceModule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = "Arduino";
    public string Address { get; set; } = string.Empty;
    public string MqttHost { get; set; } = "localhost";
    public int MqttPort { get; set; } = 1883;
    public string TelemetryTopic { get; set; } = "farm/device/telemetry";
    public string CommandTopic { get; set; } = "farm/device/command";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class ApiResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
