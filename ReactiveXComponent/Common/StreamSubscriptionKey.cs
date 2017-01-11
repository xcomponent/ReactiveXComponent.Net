using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Common
{
    public class StreamSubscriptionKey
    {
        public SubscriptionKey SubscriberKey { get; }
        public Action<MessageEventArgs> StateMachineListener { get; }

        public StreamSubscriptionKey(SubscriptionKey subscriberKey, Action<MessageEventArgs> stateMachineListener)
        {
            SubscriberKey = subscriberKey;
            StateMachineListener = stateMachineListener;
        }

        protected bool Equals(StreamSubscriptionKey other)
        {
            return SubscriberKey.Equals(other.SubscriberKey) && StateMachineListener.Equals(other.StateMachineListener);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StreamSubscriptionKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SubscriberKey.GetHashCode() * 397) ^ (StateMachineListener.GetHashCode()* 397);
            }
        }
    }
}
