using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketHeader
    {
        public WebSocketEngineHeader Header { get; set; }
        public string JsonMessage { get; set; }
    }
}
