using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReactiveXComponent.RabbitMq
{
    internal class RabbitMqSubscriberInfos
    {
        public IModel Channel { get; set; }
        public IModel ReplyChannel { get; set; }
        public bool IsOpen { get; set; }
        public EventingBasicConsumer Subscriber { get; set; }

        private bool Equals(RabbitMqSubscriberInfos other)
        {
            return Channel == other.Channel && ReplyChannel == other.ReplyChannel && IsOpen == other.IsOpen && Subscriber == other.Subscriber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RabbitMqSubscriberInfos) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Channel?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (ReplyChannel?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ IsOpen.GetHashCode();
                hashCode = (hashCode*397) ^ (Subscriber?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
