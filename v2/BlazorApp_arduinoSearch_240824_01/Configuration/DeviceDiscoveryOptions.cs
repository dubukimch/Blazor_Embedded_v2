namespace BlazorApp_arduinoSearch_240824_01.Configuration;

public class DeviceDiscoveryOptions
{
    public string BaseIpAddress { get; set; } = "172.30.1.";

    public int StartHost { get; set; } = 1;

    public int EndHost { get; set; } = 253;

    public int PingTimeoutMilliseconds { get; set; } = 1000;

    public int HttpTimeoutMilliseconds { get; set; } = 2000;

    public int MaxConcurrency { get; set; } = 32;

    public List<string> ExcludedAddresses { get; set; } = new()
    {
        "172.30.1.254"
    };

    public string NormalizedBaseIpAddress =>
        string.IsNullOrWhiteSpace(BaseIpAddress)
            ? "172.30.1."
            : BaseIpAddress.TrimEnd('.') + ".";
}
