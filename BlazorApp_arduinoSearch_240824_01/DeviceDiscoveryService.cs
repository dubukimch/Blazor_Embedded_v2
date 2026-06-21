using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using BlazorApp_arduinoSearch_240824_01.Configuration;
using BlazorApp_arduinoSearch_240824_01.Models;
using Microsoft.Extensions.Options;

namespace BlazorApp_arduinoSearch_240824_01.Services;

public class DeviceDiscoveryService
{
    private readonly HttpClient _httpClient;
    private readonly DeviceDiscoveryOptions _options;
    private readonly string _serverIpAddress;

    public DeviceDiscoveryService(HttpClient httpClient, IOptions<DeviceDiscoveryOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _serverIpAddress = GetServerIpAddress();

        var timeoutMilliseconds = Math.Max(1, _options.HttpTimeoutMilliseconds);
        _httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    public IReadOnlyList<string> GetCandidateIpAddresses()
    {
        if (_options.StartHost > _options.EndHost)
        {
            return Array.Empty<string>();
        }

        var excludedAddresses = new HashSet<string>(
            _options.ExcludedAddresses.Where(address => !string.IsNullOrWhiteSpace(address)),
            StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(_serverIpAddress))
        {
            excludedAddresses.Add(_serverIpAddress);
        }

        var addresses = new List<string>();
        var startHost = Math.Max(1, _options.StartHost);
        var endHost = Math.Min(254, _options.EndHost);
        var baseIpAddress = _options.NormalizedBaseIpAddress;

        for (var host = startHost; host <= endHost; host++)
        {
            var address = baseIpAddress + host.ToString(CultureInfo.InvariantCulture);

            if (!excludedAddresses.Contains(address))
            {
                addresses.Add(address);
            }
        }

        return addresses;
    }

    public async Task<List<Device>> DiscoverDevicesAsync(
        IProgress<DeviceDiscoveryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var candidateAddresses = GetCandidateIpAddresses();
        var devices = new ConcurrentBag<Device>();
        var total = candidateAddresses.Count;
        var scanned = 0;
        var found = 0;
        var maxConcurrency = Math.Max(1, _options.MaxConcurrency);

        ReportProgress(progress, total, 0, 0, string.Empty, "Starting device discovery.");

        using var throttler = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var tasks = candidateAddresses.Select(async ipAddress =>
        {
            await throttler.WaitAsync(cancellationToken);

            try
            {
                var device = await TryDiscoverDeviceAsync(ipAddress, cancellationToken);

                if (device != null)
                {
                    devices.Add(device);
                    Interlocked.Increment(ref found);
                }
            }
            finally
            {
                throttler.Release();
                var scannedCount = Interlocked.Increment(ref scanned);
                ReportProgress(
                    progress,
                    total,
                    scannedCount,
                    Volatile.Read(ref found),
                    ipAddress,
                    $"Scanned {scannedCount:N0} of {total:N0} addresses.");
            }
        });

        await Task.WhenAll(tasks);

        ReportProgress(progress, total, scanned, found, string.Empty, "Device discovery completed.");

        return devices
            .OrderBy(device => IPAddress.TryParse(device.Address, out var address) ? address.GetAddressBytes() : Array.Empty<byte>(), ByteArrayComparer.Instance)
            .ThenBy(device => device.Address, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<Device?> TryDiscoverDeviceAsync(string ipAddress, CancellationToken cancellationToken)
    {
        if (!await PingHostAsync(ipAddress))
        {
            return null;
        }

        return await GetDeviceInfoAsync(ipAddress, cancellationToken);
    }

    private async Task<bool> PingHostAsync(string ipAddress)
    {
        try
        {
            using var ping = new Ping();
            var timeoutMilliseconds = Math.Max(1, _options.PingTimeoutMilliseconds);
            var reply = await ping.SendPingAsync(ipAddress, timeoutMilliseconds);

            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Device> GetDeviceInfoAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"http://{ipAddress}/device_info", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorDevice(ipAddress, $"HTTP {(int)response.StatusCode}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = jsonDocument.RootElement;
            var mqttTopic = GetString(root, "mqtt_topic", "mqttTopic");

            return new Device
            {
                Name = GetString(root, "name"),
                Address = ipAddress,
                Description = GetString(root, "description"),
                MqttServer = GetString(root, "mqtt_server", "mqttServer"),
                MqttPort = GetString(root, "mqtt_port", "mqttPort"),
                MqttTopic = string.IsNullOrWhiteSpace(mqttTopic) ? GetFirstTopic(root) : mqttTopic
            };
        }
        catch (Exception ex)
        {
            return CreateErrorDevice(ipAddress, ex.Message);
        }
    }

    private static string GetServerIpAddress()
    {
        try
        {
            var host = Dns.GetHostName();
            var ipAddress = Dns.GetHostAddresses(host)
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));

            return ipAddress?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetString(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (!root.TryGetProperty(name, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.ToString(),
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    private static string GetFirstTopic(JsonElement root)
    {
        if (!root.TryGetProperty("topics", out var topics) || topics.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        foreach (var topic in topics.EnumerateObject())
        {
            if (topic.Value.ValueKind == JsonValueKind.String)
            {
                return topic.Value.GetString() ?? string.Empty;
            }

            if (topic.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in topic.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        return item.GetString() ?? string.Empty;
                    }
                }
            }
        }

        return string.Empty;
    }

    private static Device CreateErrorDevice(string ipAddress, string message)
    {
        return new Device
        {
            Address = ipAddress,
            Description = $"Error: {message}"
        };
    }

    private static void ReportProgress(
        IProgress<DeviceDiscoveryProgress>? progress,
        int total,
        int scanned,
        int found,
        string currentIpAddress,
        string message)
    {
        progress?.Report(new DeviceDiscoveryProgress
        {
            Total = total,
            Scanned = scanned,
            Found = found,
            CurrentIpAddress = currentIpAddress,
            Message = message
        });
    }

    private sealed class ByteArrayComparer : IComparer<byte[]>
    {
        public static ByteArrayComparer Instance { get; } = new();

        public int Compare(byte[]? x, byte[]? y)
        {
            x ??= Array.Empty<byte>();
            y ??= Array.Empty<byte>();

            for (var i = 0; i < Math.Min(x.Length, y.Length); i++)
            {
                var comparison = x[i].CompareTo(y[i]);

                if (comparison != 0)
                {
                    return comparison;
                }
            }

            return x.Length.CompareTo(y.Length);
        }
    }
}
