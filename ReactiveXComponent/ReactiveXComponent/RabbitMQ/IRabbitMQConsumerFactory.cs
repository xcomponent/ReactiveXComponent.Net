namespace ReactiveXComponent.RabbitMQ
{
    public interface IRabbitMQConsumerFactory
    {
        IConsumer Create(string componentName, string topic);
    }
}