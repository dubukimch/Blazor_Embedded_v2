namespace BlazorApp_arduinoSearch_240824_01.DataModel
{
    public class Device
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string MqttServer { get; set; }
        public string MqttPort { get; set; }
        public Dictionary<string, List<string>> MqttTopics { get; set; }

        public Device()
        {
            MqttTopics = new Dictionary<string, List<string>>();
        }
    }
}