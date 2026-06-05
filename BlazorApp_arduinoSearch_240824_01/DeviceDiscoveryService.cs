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

    public DeviceDiscoveryService(HttpClient httpClient, IOptions<DeviceDiscoveryOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _serverIpAddress = GetServerIpAddress();
    }

    public async Task<List<Device>> DiscoverDevicesAsync(
        IProgress<DeviceDiscoveryProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var candidates = GetCandidateIpAddresses();
        var devices = new ConcurrentBag<Device>();
        var scanned = 0;
        var maxConcurrency = Math.Max(1, _options.MaxConcurrency);

        progress?.Report(new DeviceDiscoveryProgress
        {
            Total = candidates.Count,
            Scanned = 0,
            Found = 0,
            Message = "장비 검색을 시작합니다."
        });

        using var semaphore = new SemaphoreSlim(maxConcurrency);

        var tasks = candidates.Select(async ipAddress =>
        {
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                if (await PingHost(ipAddress))
                {
                    var deviceInfo = await GetDeviceInfo(ipAddress, cancellationToken);

                    if (deviceInfo != null)
                    {
                        devices.Add(deviceInfo);
                    }
                }
            }
            finally
            {
                var currentScanned = Interlocked.Increment(ref scanned);

                progress?.Report(new DeviceDiscoveryProgress
                {
                    Total = candidates.Count,
                    Scanned = currentScanned,
                    Found = devices.Count,
                    CurrentIpAddress = ipAddress,
                    Message = $"{currentScanned}/{candidates.Count} 검색 완료"
                });

                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        return devices
            .OrderBy(device => device.Address)
            .ToList();
    }

    internal IReadOnlyList<string> GetCandidateIpAddresses()
    {
        if (_options.StartHost > _options.EndHost)
        {
            return Array.Empty<string>();
        }

        var baseIp = _options.NormalizedBaseIpAddress;
        var excludedAddresses = new HashSet<string>(_options.ExcludedAddresses, StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(_serverIpAddress))
        {
            excludedAddresses.Add(_serverIpAddress);
        }

        return Enumerable.Range(_options.StartHost, _options.EndHost - _options.StartHost + 1)
            .Select(host => baseIp + host)
            .Where(ipAddress => !excludedAddresses.Contains(ipAddress))
            .ToList();
    }

    private static string GetServerIpAddress()
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

    private async Task<Device?> GetDeviceInfo(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(_options.HttpTimeoutMilliseconds);

            var response = await _httpClient.GetAsync($"http://{ipAddress}/device_info", timeoutSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                return CreateErrorDevice(ipAddress, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var device = await response.Content.ReadFromJsonAsync<Device>(cancellationToken: timeoutSource.Token);

            if (device == null)
            {
                return CreateErrorDevice(ipAddress, "Unable to deserialize device information.");
            }

            device.Address = ipAddress;
            return device;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return CreateErrorDevice(ipAddress, $"Timeout after {_options.HttpTimeoutMilliseconds}ms.");
        }
        catch (HttpRequestException ex)
        {
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
