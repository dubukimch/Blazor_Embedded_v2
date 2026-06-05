namespace BlazorApp_arduinoSearch_240824_01.Models;

public class DeviceDiscoveryProgress
{
    public int Total { get; set; }

    public int Scanned { get; set; }

    public int Found { get; set; }

    public string CurrentIpAddress { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
