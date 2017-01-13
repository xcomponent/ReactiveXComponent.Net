
namespace ReactiveXComponent.Common
{
    public class StateMachineRefHeader
    {
        public long StateMachineId { get; set; }

        public int AgentId { get; set; }

        public  int StateCode { get; set; }

        public long StateMachineCode { get; set; }

        public long ComponentCode { get; set; }

        public int EventCode { get; set; }

        public string MessageType { get; set; }

        public string PublishTopic { get; set; }


        public override bool Equals(object obj)
        {
            var toCompareWith = obj as StateMachineRefHeader;

            return toCompareWith != null && Equals(toCompareWith);
        }

        private bool Equals(StateMachineRefHeader other)
        {
            return StateMachineId == other.StateMachineId 
                && AgentId == other.AgentId
                && StateCode == other.StateCode 
                && StateMachineCode == other.StateMachineCode 
                && ComponentCode == other.ComponentCode
                && EventCode == other.EventCode
                && string.Equals(MessageType, other.MessageType)
                && string.Equals(PublishTopic, other.PublishTopic);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) StateMachineId;
                hashCode = (int) ((hashCode * 397) ^ AgentId);
                hashCode = (int)((hashCode * 397) ^ StateCode);
                hashCode = (int) ((hashCode * 397) ^ StateMachineCode);
                hashCode = (int) ((hashCode * 397) ^ ComponentCode);
                hashCode = (hashCode * 397) ^ EventCode;
                hashCode = (hashCode * 397) ^ (MessageType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (PublishTopic?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
