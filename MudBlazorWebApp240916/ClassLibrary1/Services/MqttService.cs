using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet;
using MudBlazorWebApp240916.Shared.DataModel;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

namespace MudBlazorWebApp240916.Shared.Services
{
    public class MqttService : IAsyncDisposable
    {
        private IMqttClient _client;
        public event Action<string, string> OnMessageReceived;
        private readonly HttpClient _httpClient;
        private Dictionary<string, List<string>> _subscribedTopics;

        public MqttService (HttpClient httpClient)
        {
            _httpClient = httpClient;
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

            _subscribedTopics = new Dictionary<string, List<string>>();
        }

        public bool IsConnected => _client.IsConnected;

        // 구독 메서드 정의
        public async Task SubscribeAsync (string topic)
        {
            if (!IsConnected)
            {
                Console.WriteLine("MQTT 클라이언트가 연결되지 않았습니다.");
                return;
            }

            await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build());

            Console.WriteLine($"Subscribed to topic: {topic}");
        }

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

            foreach (var topic in mqttTopics.Values.SelectMany(v => v))
            {
                await SubscribeAsync(topic); // 구독 작업을 호출
            }

            _subscribedTopics = mqttTopics;
        }

        public async Task<Dictionary<string, List<string>>> ConnectAndGetTopicsAsync (string mqttServer, string ipAddress, int port)
        {
            if (_client.IsConnected)
            {
                Console.WriteLine("이미 연결됨");
                return _subscribedTopics;
            }

            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(mqttServer, port)
                .Build();

            await _client.ConnectAsync(options);

            try
            {
                var response = await _httpClient.GetAsync($"http://{ipAddress}/device_info");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to retrieve topics from device. Status Code: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                var deviceInfo = new Device
                {
                    Name = jsonDoc.RootElement.GetProperty("name").GetString(),
                    Address = ipAddress,
                    Description = jsonDoc.RootElement.GetProperty("description").GetString(),
                    MqttTopics = jsonDoc.RootElement
                        .GetProperty("topics")
                        .EnumerateObject()
                        .ToDictionary(x => x.Name, x => new List<string> { x.Value.GetString() })
                };

                var mqttTopics = deviceInfo.MqttTopics;

                foreach (var topic in mqttTopics.Values)
                {
                    await SubscribeAsync(topic.First()); // 구독 메서드 호출
                }

                _subscribedTopics = mqttTopics;
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

        public async Task PublishMessageAsync (string topic, string message)
        {
            var messageBuilder = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .WithRetainFlag(false);

            await _client.PublishAsync(messageBuilder.Build());
        }

        public async Task DisconnectAsync ()
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync();
                Console.WriteLine("Disconnected from MQTT Broker.");
            }
        }

        public async ValueTask DisposeAsync ()
        {
            await DisconnectAsync();
        }
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
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"http://{ipAddress}/configure_mqtt"),
                    Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
                };

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
}
