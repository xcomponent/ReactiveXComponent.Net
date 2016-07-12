
namespace ReactiveXComponent.Common
{
    public class StateMachineRef
    {
        public long StateMachineId { get; set; }

        public long AgentId { get; set; }

        public long StateMachineCode { get; set; }

        public long ComponentCode { get; set; }

        public int EventCode { get; set; }

        public string MessageType { get; set; }

        public string PublishTopic { get; set; }

        public override bool Equals(object obj)
        {
            var toCompareWith = obj as StateMachineRef;

            return toCompareWith != null && Equals(toCompareWith);
        }

        private bool Equals(StateMachineRef other)
        {
            return StateMachineId == other.StateMachineId &&
                   AgentId == other.AgentId &&
                   StateMachineCode == other.StateMachineCode &&
                   ComponentCode == other.ComponentCode &&
                   EventCode == other.EventCode &&
                   MessageType == other.MessageType &&
                   PublishTopic == other.PublishTopic;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StateMachineId.GetHashCode();
                hashCode = (hashCode*397) ^ AgentId.GetHashCode();
                hashCode = (hashCode * 397) ^ StateMachineCode.GetHashCode();
                hashCode = (hashCode * 397) ^ ComponentCode.GetHashCode();
                hashCode = (hashCode * 397) ^ EventCode.GetHashCode();
                hashCode = (hashCode * 397) ^ (MessageType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (PublishTopic?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
