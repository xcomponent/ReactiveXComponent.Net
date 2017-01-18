
namespace ReactiveXComponent.WebSocket
{
    public class WebSocketMessage
    {
        public string Command { get; }
        public string Topic { get; }
        public string Json { get; }
        public string ComponentCode { get; }

        public WebSocketMessage(string command, string topic, string json, string componentCode)
        {
            Command = command;
            Topic = topic;
            Json = json;
            ComponentCode = componentCode;
        }
    }
}
