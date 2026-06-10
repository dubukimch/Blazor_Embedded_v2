namespace BlazorApp_arduinoSearch_240824_01.Models;

public sealed record MqttConnectionParameters(string Server, int Port, string Topic)
{
    public static MqttConnectionParameters Empty { get; } = new(string.Empty, 0, string.Empty);
}
