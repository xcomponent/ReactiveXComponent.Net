namespace ReactiveXComponent.XCApi
{
    internal class ConsumerKey
    {
        private readonly long _componentCode;
        private readonly long _stateMachineCode;

        public long ComponentCode
        {
            get { return _componentCode; }
        }

        public long StateMachineCode
        {
            get { return _stateMachineCode; }
        }

        public ConsumerKey(long componentCode, long stateMachineCode)
        {
            _componentCode = componentCode;
            _stateMachineCode = stateMachineCode;
        }

        protected bool Equals(ConsumerKey other)
        {
            return ComponentCode == other.ComponentCode && StateMachineCode == other.StateMachineCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConsumerKey) obj);
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
