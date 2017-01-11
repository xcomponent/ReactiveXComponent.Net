using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketCommand
    {
        public const string Error = "error";
        public const string Input = "input";
        public const string Update = "update";
        public const string Snapshot = "snapshot";
        public const string Subscribe = "subscribe";
        public const string Unsubscribe = "unsubscribe";
    }
}
