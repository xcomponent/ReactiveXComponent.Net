namespace ReactiveXComponent.Parser
{
    public class TopicIdentifier
    {
        protected bool Equals(TopicIdentifier other)
        {
            return Component == other.Component && StateMachine == other.StateMachine && string.Equals(TopicType, other.TopicType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TopicIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Component.GetHashCode();
                hashCode = (hashCode*397) ^ StateMachine.GetHashCode();
                hashCode = (hashCode*397) ^ (TopicType != null ? TopicType.GetHashCode() : 0);
                return hashCode;
            }
        }

        public TopicIdentifier(long component, long stateMachine, string topicType)
        {
            Component = component;
            StateMachine = stateMachine;
            TopicType = topicType;
        }

        public long Component { get; }
        public long StateMachine { get; }
        public  string TopicType { get; }
    }
}
