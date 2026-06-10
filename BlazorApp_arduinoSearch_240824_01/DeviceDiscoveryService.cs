
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using BlazorApp_arduinoSearch_240824_01.Configuration;
using BlazorApp_arduinoSearch_240824_01.Models;
using Microsoft.Extensions.Options;

namespace BlazorApp_arduinoSearch_240824_01.Services;


public class DeviceDiscoveryService
{
    private readonly HttpClient _httpClient;
    private readonly DeviceDiscoveryOptions _options;
    private readonly string _serverIpAddress;

    public DeviceDiscoveryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _serverIpAddress = GetServerIpAddress();
    }

    private string GetServerIpAddress()

    {
        try
        {
            var host = Dns.GetHostName();
            var ipAddresses = Dns.GetHostAddresses(host);
            var ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            return ipv4Address?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<List<Device>> DiscoverDevicesAsync()
    {
        var devices = new List<Device>();
        var tasks = new List<Task>();

        var baseIp = "172.30.1."; // 기본 IP 범위 설정

        for (int i = 1; i <= 253; i++)
        {
            var ipAddress = baseIp + i;
            if (ipAddress == _serverIpAddress || ipAddress == baseIp + "254")
            {
                continue; // 서버 자신과 254번 IP는 제외
            }

            tasks.Add(Task.Run(async () =>
            {
                if (await PingHost(ipAddress))
                {
                    var deviceInfo = await GetDeviceInfo(ipAddress);
                    if (deviceInfo != null)
                    {
                        lock (devices)
                        {
                            devices.Add(deviceInfo);
                        }
                    }
                    else
                    {
                        lock (devices)
                        {
                            devices.Add(new Device
                            {
                                Address = ipAddress,
                                Description = $"Error: Unable to retrieve information from {ipAddress}"
                            });
                        }
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
        return devices;
    }


    private async Task<bool> PingHost(string ipAddress)
    {
        try
        {

            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, _options.PingTimeoutMilliseconds);

            return reply.Status == IPStatus.Success;

        }
        catch
        {
            return false;
        }
    }


    private async Task<Device> GetDeviceInfo(string ipAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://{ipAddress}/device_info");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Received device info: {jsonString}");

                // 수동으로 JSON 파싱
                var jsonDoc = JsonDocument.Parse(jsonString);
                var device = new Device
                {
                    Name = jsonDoc.RootElement.GetProperty("name").GetString(),
                    Address = ipAddress,
                    Description = jsonDoc.RootElement.GetProperty("description").GetString(),
                    MqttTopics = jsonDoc.RootElement
                    .GetProperty("topics")
                    .EnumerateObject()
                    .ToDictionary(
                        x => x.Name,
                        x => new List<string> { x.Value.GetString() } // List<string>으로 변환
                    )
                };

            return CreateErrorDevice(ipAddress, ex.Message);
        }
        catch (Exception ex)
        {
            return CreateErrorDevice(ipAddress, ex.Message);

        }
    }

    private static Device CreateErrorDevice(string ipAddress, string message)
    {
        return new Device
        {
            Address = ipAddress,
            Description = $"Error: {message}"
        };
    }
}
