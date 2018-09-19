
using System;

namespace ReactiveXComponent.Common
{
    public class StateMachineRefHeader
    {
        public string StateMachineId { get; set; }

        public int StateCode { get; set; }

        public int StateMachineCode { get; set; }

        public int ComponentCode { get; set; }

        public string MessageType { get; set; }

        public string PrivateTopic { get; set; }

        public string SessionData { get; set; }

        public string ErrorMessage { get; set; }

        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        public int WorkerId { get; set; }
    }
}
