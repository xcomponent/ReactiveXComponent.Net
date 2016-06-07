namespace ReactiveXComponent.RabbitMQ
{
    public interface IRabbitMqPublisherFactory
    {
        IRabbitMqPublisher Create(string componentName);
    }
}