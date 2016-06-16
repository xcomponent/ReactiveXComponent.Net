namespace ReactiveXComponent.Parser
{
    public class TopicIdentifier
    {
        public long Component { get; set; }
        public long StateMachine { get; set; }
        public int EventCode { get; set; }
        public string TopicType { get; set; }

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
