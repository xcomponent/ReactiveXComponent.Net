using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.Common
{
    public class WebSocketEndpoint
    {
        public WebSocketEndpoint(string name, string host, string port, WebSocketType type)
        {
            Name = name;
            Host = host;
            Port = port;
            Type = type;
        }

        public string Name { get; private set; }

        public string Host { get; set; }

        public string Port { get; set; }

        public WebSocketType Type { get; set; }
    }
}
