using System.Collections.Generic;

namespace MudBlazorWebApp240916.Shared.DataModel
{
    public class Device
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string MqttServer { get; set; }
        public string MqttPort { get; set; }
        public Dictionary<string, List<string>> MqttTopics { get; set; }
        // 트리 구조를 위한 속성 추가
        public TreeNodeModel TopicTreeRootNode { get; set; }  // MQTT 토픽 트리 루트 노드
        public TreeNodeModel SelectedTopicNode { get; set; }  // 선택된 MQTT 토픽 노드
        public Device ()
        {
            MqttTopics = new Dictionary<string, List<string>>();
        }
    }
    public class TreeNodeModel
    {
        public string Name { get; set; }
        public List<TreeNodeModel> Nodes { get; set; } = new List<TreeNodeModel>();
    }
}