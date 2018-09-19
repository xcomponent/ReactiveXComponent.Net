using System;

namespace ReactiveXComponent.Common
{
    public class Header
    {
        public long StateMachineCode { get; set; }

        public long ComponentCode { get; set; }

        public int EventCode { get; set; }

        public int StateCode { get; set; }

        public int IncomingEventType { get; set; }

        public string MessageType { get; set; }

        public string PublishTopic { get; set; }

        public string ErrorMessage { get; set; }

        public string MessageId { get; set; } = Guid.NewGuid().ToString();
    }
}
