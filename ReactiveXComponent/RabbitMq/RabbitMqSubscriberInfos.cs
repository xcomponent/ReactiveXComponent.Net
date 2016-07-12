using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSubscriberInfos
    {
        public IModel Channel { get; set; }
        public EventingBasicConsumer Subscriber { get; set; }

        private bool Equals(RabbitMqSubscriberInfos other)
        {
            return Channel == other.Channel && Subscriber == other.Subscriber;
        }

        public override bool Equals(object obj)
        {
            var toCompareWith = obj as RabbitMqSubscriberInfos;

            return toCompareWith != null && Equals(toCompareWith);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Channel?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Subscriber?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
