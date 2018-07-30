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
        public string StateMachineId { get; set; }

        public int? StateMachineCode { get; set; }

        public int ComponentCode { get; set; }

        public int? StateCode { get; set; }

        public int? EngineCode { get; set; }

        public int EventCode { get; set; }

        public int IncomingEventType { get; set; }

        public long? SessionId { get; set; }

        public string SessionData { get; set; }

        public string MessageType { get; set; }

        public string PublishTopic { get; set; }

        public string ErrorMessage { get; set; }
    }
}
