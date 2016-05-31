namespace ReactiveXComponent.RabbitMQ
{
    public interface IRabbitMqConsumerFactory
    {
        IConsumer Create(string componentName, string topic);
    }
}