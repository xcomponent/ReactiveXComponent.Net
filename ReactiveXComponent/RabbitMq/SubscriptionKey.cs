namespace ReactiveXComponent.RabbitMq
{
    public class SubscriptionKey
    {
        private long ComponentCode { get; }

        private long StateMachineCode { get; }

        private string RoutingKey { get; }

        public SubscriptionKey(long componentCode, long stateMachineCode, string routingKey)
        {
            ComponentCode = componentCode;
            StateMachineCode = stateMachineCode;
            RoutingKey = routingKey;
        }

        protected bool Equals(SubscriptionKey other)
        {
            return ComponentCode == other.ComponentCode && StateMachineCode == other.StateMachineCode && RoutingKey == other.RoutingKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SubscriptionKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (((ComponentCode.GetHashCode() * 397) ^ StateMachineCode.GetHashCode()) * 397) ^ (RoutingKey?.GetHashCode() ?? 0);
            }
        }
    }
}
