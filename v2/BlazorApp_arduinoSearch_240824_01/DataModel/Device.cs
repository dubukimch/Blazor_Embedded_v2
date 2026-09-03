namespace BlazorApp_arduinoSearch_240824_01.DataModel
{
    public class Device
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MqttServer { get; set; } = string.Empty;
        public string MqttPort { get; set; } = string.Empty;
        public Dictionary<string, List<string>> MqttTopics { get; set; } = new();
        public TreeNodeModel? TopicTreeRootNode { get; set; }
        public TreeNodeModel? SelectedTopicNode { get; set; }
    }
    public class TreeNodeModel
    {
        public string Name { get; set; } = string.Empty;
        public List<TreeNodeModel> Nodes { get; set; } = new();
    }
}
