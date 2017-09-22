
namespace ReactiveXComponent.Common
{
    public class StateMachineRefHeader
    {
        protected bool Equals(StateMachineRefHeader other)
        {
            return StateMachineId == other.StateMachineId && StateCode == other.StateCode && StateMachineCode == other.StateMachineCode && ComponentCode == other.ComponentCode && string.Equals(MessageType, other.MessageType) && string.Equals(PrivateTopic, other.PrivateTopic) && string.Equals(SessionData, other.SessionData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StateMachineRefHeader) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StateMachineId.GetHashCode();
                hashCode = (hashCode * 397) ^ StateCode;
                hashCode = (hashCode * 397) ^ StateMachineCode;
                hashCode = (hashCode * 397) ^ ComponentCode;
                hashCode = (hashCode * 397) ^ (MessageType != null ? MessageType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PrivateTopic != null ? PrivateTopic.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SessionData != null ? SessionData.GetHashCode() : 0);
                return hashCode;
            }
        }

        public long StateMachineId { get; set; }

        public int StateCode { get; set; }

        public int StateMachineCode { get; set; }

        public int ComponentCode { get; set; }

        public string MessageType { get; set; }

        public string PrivateTopic { get; set; }

        public string SessionData { get; set; }
    }
}
