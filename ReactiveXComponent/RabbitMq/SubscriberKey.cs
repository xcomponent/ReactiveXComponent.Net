namespace ReactiveXComponent.RabbitMq
{
    public class SubscriberKey
    {
        private long ComponentCode { get; }

        private long StateMachineCode { get; }


        public SubscriberKey(long componentCode, long stateMachineCode)
        {
            ComponentCode = componentCode;
            StateMachineCode = stateMachineCode;
        }

        protected bool Equals(SubscriberKey other)
        {
            return ComponentCode == other.ComponentCode && StateMachineCode == other.StateMachineCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SubscriberKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ComponentCode.GetHashCode()*397) ^ StateMachineCode.GetHashCode();
            }
        }
    }
}
