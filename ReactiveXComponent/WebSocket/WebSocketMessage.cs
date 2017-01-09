using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketMessage
    {
        public WebSocketMessage(string command, string topic, string json, string componentCode)
        {
            Command = command;
            Topic = topic;
            Json = json;
            ComponentCode = componentCode;
        }

        public string Command { get; }
        public string Topic { get; }
        public string Json { get; }
        public string ComponentCode { get; }
    }
}
