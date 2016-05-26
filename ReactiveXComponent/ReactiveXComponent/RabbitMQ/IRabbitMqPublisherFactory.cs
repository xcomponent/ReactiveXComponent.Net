namespace ReactiveXComponent.RabbitMQ
{
    public interface IRabbitMqPublisherFactory
    {
        IRabbitMQPublisher Create(string componentName);
    }
}