namespace BlazorApp_arduinoSearch_240824_01.Configuration;

public class MqttConnectionOptions
{
    public int MaxReconnectAttempts { get; set; } = 3;

    public int ReconnectDelayMilliseconds { get; set; } = 2000;
}
