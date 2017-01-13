
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

        public override bool Equals(object obj)
        {
            var toCompareWith = obj as WebSocketMessage;
            return toCompareWith != null && Equals(toCompareWith);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Command?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Topic?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Json?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (ComponentCode?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        private bool Equals(WebSocketMessage webSocketMessage)
        {
            return string.Equals(Command, webSocketMessage.Command) &&
                   string.Equals(Topic, webSocketMessage.Topic) &&
                   string.Equals(Json, webSocketMessage.Json) &&
                   string.Equals(ComponentCode, webSocketMessage.ComponentCode);
        }
    }
}
