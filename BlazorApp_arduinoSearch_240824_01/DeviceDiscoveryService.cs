using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public class DeviceDiscoveryService
{
    private readonly HttpClient _httpClient;
    private readonly string _serverIpAddress;

    public DeviceDiscoveryService (HttpClient httpClient)
    {
        _httpClient = httpClient;

        // 서버의 IP 주소 가져오기
        _serverIpAddress = GetServerIpAddress();
    }
    private string GetServerIpAddress ()
    {
        try
        {
            var host = Dns.GetHostName();
            var ipAddresses = Dns.GetHostAddresses(host);

            // IPv4 주소만 반환
            var ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return ipv4Address?.ToString() ?? "";
        }
        catch
        {
            return "";
        }
    }

    public async Task<List<Device>> DiscoverDevicesAsync ()
    {
        var devices = new List<Device>();
        var tasks = new List<Task>();

        var baseIp = "172.30.1.";  // 네트워크의 기본 IP 범위

        for (int i = 1; i <= 253; i++)
        {
            var ipAddress = baseIp + i;
            if (ipAddress == _serverIpAddress || ipAddress == baseIp + "254")
            {
                continue; // 이 IP 주소는 검색 대상에서 제외
            }
            // 병렬로 Ping 및 장치 정보 검색 수행
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

    private async Task<bool> PingHost (string ipAddress)
    {
        try
        {
            using (var ping = new Ping())
            {
                var reply = await ping.SendPingAsync(ipAddress, 1000); // 3초 대기 시간 제한
                return reply.Status == IPStatus.Success;
            }
        }
        catch
        {
            return false;
        }
    }

    private async Task<Device> GetDeviceInfo (string ipAddress)
    {
        try
        {
            // HTTP 요청을 통해 장치 정보 가져오기
            var response = await _httpClient.GetAsync($"http://{ipAddress}/device_info");
            if (response.IsSuccessStatusCode)
            {
                var device = await response.Content.ReadFromJsonAsync<Device>();
                device.Address = ipAddress;
                return device;
            }
        }
        catch (HttpRequestException ex)
        {
            // HTTP 요청 실패 시 예외 메시지를 장치 설명에 추가
            return new Device
            {
                Address = ipAddress,
                Description = $"Error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            // 일반적인 예외 처리
            return new Device
            {
                Address = ipAddress,
                Description = $"Error: {ex.Message}"
            };
        }

        return null;
    }
}

public class Device
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public string MqttServer { get; set; }
    public string MqttPort { get; set; }
    public string MqttTopic { get; set; }
}
