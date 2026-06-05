using System.Text.Json.Serialization;

namespace BlazorApp_arduinoSearch_240824_01.Models;

public class Device
{
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MqttServer { get; set; } = string.Empty;

    public string MqttPort { get; set; } = string.Empty;

    public string MqttTopic { get; set; } = string.Empty;

    [JsonIgnore]
    public bool HasError => Description?.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ?? false;
}
