namespace ReactiveXComponent.Parser
{
    public class TopicIdentifier
    {
        public TopicIdentifier(long component, long stateMachine, int eventCode, string topicType)
        {
            Component = component;
            StateMachine = stateMachine;
            EventCode = eventCode;
            TopicType = topicType;
        }

        public long Component { get; }
        public long StateMachine { get; }
        public int EventCode { get; }
        public  string TopicType { get; }

        public override bool Equals(object obj)
        {
            var toCompareWith = obj as TopicIdentifier;

            return toCompareWith != null && Equals(toCompareWith);
        }

        private bool Equals(TopicIdentifier other)
        {
            return Component == other.Component &&
                   StateMachine == other.StateMachine &&
                   EventCode == other.EventCode &&
                   TopicType == other.TopicType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Component.GetHashCode();
                hashCode = (hashCode * 397) ^ StateMachine.GetHashCode();
                hashCode = (hashCode * 397) ^ EventCode.GetHashCode();
                hashCode = (hashCode * 397) ^ (TopicType?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
