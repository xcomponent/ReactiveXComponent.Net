using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSubscriberInfos
    {
        public IModel Channel { get; set; }
        public EventingBasicConsumer Subscriber { get; set; }
    }
}
