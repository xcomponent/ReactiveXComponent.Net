using System;

namespace ReactiveXComponent.RabbitMq
{
    public class SubscriptionKey
    {
        public SubscriberKey SubscriberKey { get; }
        public Action<MessageEventArgs> StateMachineListener { get; }

        public SubscriptionKey(SubscriberKey subscriberKey, Action<MessageEventArgs> stateMachineListener)
        {
            SubscriberKey = subscriberKey;
            StateMachineListener = stateMachineListener;
        }

        protected bool Equals(SubscriptionKey other)
        {
            return SubscriberKey.Equals(other.SubscriberKey) && StateMachineListener.Equals(other.StateMachineListener);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SubscriptionKey)obj);
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
