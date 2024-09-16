using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet;
using System.Text.Json;
using System.Text;
using MudBlazorWebApp240916.Shared.DataModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System;
namespace MudBlazorWebApp240916.Shared.Services
{
    public class MqttService
    {
        private IMqttClient _client;
        public event Action<string, string> OnMessageReceived;

        private readonly HttpClient _httpClient;
        private Dictionary<string, List<string>> _subscribedTopics; // 구독된 토픽 정보를 저장할 변수

        public MqttService (HttpClient httpClient)
        {
            _httpClient = httpClient; // DI로 HttpClient 주입
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _client.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected to MQTT Broker.");
            };

            _client.DisconnectedAsync += e =>
            {
                Console.WriteLine("Disconnected from MQTT Broker.");
                return Task.CompletedTask;
            };

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                OnMessageReceived?.Invoke(topic, message);
            };

            _subscribedTopics = new Dictionary<string, List<string>>(); // 초기화
        }

        public bool IsConnected => _client.IsConnected;

        public void Dispose ()
        {
            if (_client.IsConnected)
            {
                _client.DisconnectAsync().Wait();  // 연결이 되어 있으면 끊기
            }
        }

        // MQTT 연결 메서드
        public async Task ConnectAsync (string server, int port, Dictionary<string, List<string>> mqttTopics)
        {
            if (_client.IsConnected)
            {
                Console.WriteLine("이미 연결됨");
                return;
            }

            var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer(server, port)
            .Build();

            await _client.ConnectAsync(options);

            // MQTT 토픽 구독
            foreach (var topic in mqttTopics.Values.SelectMany(v => v))
            {
                await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(topic.ToString())
                    .Build());
                Console.WriteLine($"Subscribed to topic: {topic}");
            }

            _subscribedTopics = mqttTopics; // 구독한 토픽 저장
        }

        // MQTT 토픽 구독 및 정보 가져오기 메서드
        public async Task<Dictionary<string, List<string>>> ConnectAndGetTopicsAsync (string mqttServer, string ipAddress, int port)
        {
            if (_client.IsConnected)
            {
                Console.WriteLine("이미 연결됨");
                return _subscribedTopics; // 이미 구독된 토픽 정보를 반환
            }

            var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer(mqttServer, port)
            .Build();

            await _client.ConnectAsync(options);

            // HTTP로 토픽 정보를 요청하여 수신
            try
            {
                // 타임아웃 설정
                //_httpClient.Timeout = TimeSpan.FromSeconds(10);
                var response = await _httpClient.GetAsync($"http://{ipAddress}/device_info");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to retrieve topics from device. Status Code: {response.StatusCode}");
                    return null;
                }

                // 응답 내용 읽기
                var content = await response.Content.ReadAsStringAsync();
                //var deviceInfo = JsonSerializer.Deserialize<DeviceInfoResponse>(content);

                var jsonDoc = JsonDocument.Parse(content);
                var deviceInfo = new Device
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
                if (deviceInfo == null)
                {
                    Console.WriteLine("Failed to deserialize the device info.");
                    return null;
                }
                var mqttTopics = deviceInfo.MqttTopics;
                //var mqttTopics = deviceInfo.Topics.ToDictionary(
                //    x => x.Key,
                //    x => new List<string> { x.Value }
                //);

                // MQTT 토픽 구독
                foreach (var topic in mqttTopics.Values)
                {
                    await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                        .WithTopicFilter(topic.First())
                        .Build());
                    Console.WriteLine($"Subscribed to topic: {topic}");
                }

                _subscribedTopics = mqttTopics;  // 구독한 토픽 저장
                return mqttTopics;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP 요청 오류: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"알 수 없는 오류: {ex.Message}");
                return null;
            }
        }

        // MQTT 메시지 발행
        public async Task PublishMessageAsync (string topic, string message)
        {
            var messageBuilder = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .WithRetainFlag(false);

            await _client.PublishAsync(messageBuilder.Build());
        }

        // Arduino로 POST 요청 보내기
        public async Task SendPostRequestToArduino (string ipAddress, string mqttServer, int mqttPort, Dictionary<string, List<string>> mqttTopics)
        {
            var data = new
            {
                mqtt_server = mqttServer,
                mqtt_port = mqttPort,
                mqtt_topic = mqttTopics
            };

            try
            {
                // 요청 메시지를 수동으로 작성
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"http://{ipAddress}/configure_mqtt"),
                    Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
                };

                // HttpRequestMessage에서 Content-Length를 자동으로 처리합니다.
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("POST 요청 성공");
                }
                else
                {
                    Console.WriteLine($"POST 요청 실패. Status Code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"POST 요청 오류: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"알 수 없는 오류: {ex.Message}");
            }
        }
    }

    // DeviceInfoResponse 클래스 정의
    public class DeviceInfoResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public Dictionary<string, string> Topics { get; set; }
    }
}