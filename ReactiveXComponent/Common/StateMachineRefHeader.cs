
namespace ReactiveXComponent.Common
{
    public class StateMachineRefHeader
    {
        public long StateMachineId { get; set; }

        public long AgentId { get; set; }

        public long StateMachineCode { get; set; }

        public long ComponentCode { get; set; }

        public int EventCode { get; set; }

        public string MessageType { get; set; }

        public string PublishTopic { get; set; }

        public string RoutingKey { get; set; }
    }
}
