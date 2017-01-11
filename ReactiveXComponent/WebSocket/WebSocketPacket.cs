using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketPacket
    {
        public WebSocketEngineHeader Header { get; set; }

        public string JsonMessage { get; set; }
    }
}
