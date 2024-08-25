using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace BlazorApp_arduinoSearch_240824_01.Services
{
    public class MqttService
    {
        private IMqttClient _client;
        public event Action<string, string> OnMessageReceived;
        private bool _disposed = false;
        public MqttService ()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _client.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected to MQTT Broker.");
                // Subscribing to relevant topics, for example, for status updates
                await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("home/led/status")
                    .Build());
                Console.WriteLine("Subscribed to topic: home/led/status");
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
        }
        public bool IsConnected => _client.IsConnected;
        public async Task ConnectAsync (string server, int port)
        {
            //var factory = new MqttFactory();
            //_client = factory.CreateMqttClient();
            if (_client.IsConnected)
            {
                Console.WriteLine("이미 연결된 상태에서는 연결을 시도하지 않음");// 이미 연결된 상태에서는 연결을 시도하지 않음
                return;
            }
            var options = new MqttClientOptionsBuilder()
                                        .WithClientId("BlazorClient")
                                        .WithTcpServer(server, port)
                                        .Build();

            //_client.ConnectedAsync += async e =>
            //{
            //    Console.WriteLine("Connected to MQTT Broker.");
            //    // Subscribing to relevant topics, for example, for status updates
            //    await _client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
            //        .WithTopicFilter("home/led/status")
            //        .Build());
            //};

            //_client.DisconnectedAsync += e =>
            //{
            //    Console.WriteLine("Disconnected from MQTT Broker.");
            //    return Task.CompletedTask;
            //};

            //_client.ApplicationMessageReceivedAsync += async e =>
            //{
            //    string topic = e.ApplicationMessage.Topic;
            //    string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            //    OnMessageReceived?.Invoke(topic, message);
            //};


            await _client.ConnectAsync(options);
        }

        public async Task PublishMessageAsync (string topic, string message)
        {
            var messageBuilder = new MqttApplicationMessageBuilder()
                                        .WithTopic(topic)
                                        .WithPayload(message)
                                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                                        .WithRetainFlag(false);

            await _client.PublishAsync(messageBuilder.Build());
        }

        public void Dispose ()
        {
            _disposed = true;
            _client?.Dispose(); // 리소스 정리
        }

    }
}
