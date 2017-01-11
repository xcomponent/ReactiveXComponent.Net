using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.Common
{
    /// <summary>
    /// This class is a sort of copy of the XComponent engine header.
    /// The purpose is to be able to deserialize incoming messages from the
    /// WebSocket bridge.
    /// </summary>
    public class WebSocketEngineHeader
    {
        public Option<long> StateMachineId { get; set; }

        public Option<long> StateMachineCode { get; set; }

        public Option<long> ComponentCode { get; set; }

        public Option<int> StateCode { get; set; }

        public Option<int> EngineCode { get; set; }

        public int EventCode { get; set; }

        public long[] Probes { get; set; }

        public int IncomingType { get; set; }

        public Option<int> AgentId { get; set; }

        public Option<long> SessionId { get; set; }

        public Option<string> SessionData { get; set; }

        public Option<string> MessageType { get; set; }

        public Option<string> PublishTopic { get; set; }
    }
}
