using BlazorApp_arduinoSearch_240824_01.Configuration;
using BlazorApp_arduinoSearch_240824_01.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace BlazorApp_arduinoSearch_240824_01.Tests;

public class DeviceDiscoveryServiceTests
{
    [Fact]
    public void GetCandidateIpAddresses_AppliesRangeAndExclusions()
    {
        using var httpClient = new HttpClient();
        var options = Options.Create(new DeviceDiscoveryOptions
        {
            BaseIpAddress = "192.0.2.",
            StartHost = 1,
            EndHost = 3,
            ExcludedAddresses = new List<string>
            {
                "192.0.2.2"
            }
        });
        var service = new DeviceDiscoveryService(httpClient, options);

        var result = service.GetCandidateIpAddresses();

        Assert.Equal(new[] { "192.0.2.1", "192.0.2.3" }, result);
    }

    [Fact]
    public void GetCandidateIpAddresses_ReturnsEmptyListForInvalidRange()
    {
        using var httpClient = new HttpClient();
        var options = Options.Create(new DeviceDiscoveryOptions
        {
            BaseIpAddress = "192.0.2.",
            StartHost = 10,
            EndHost = 1
        });
        var service = new DeviceDiscoveryService(httpClient, options);

        var result = service.GetCandidateIpAddresses();

        Assert.Empty(result);
    }
}
