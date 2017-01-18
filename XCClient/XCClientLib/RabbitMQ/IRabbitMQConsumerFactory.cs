namespace XCClientLib.RabbitMQ
{
    public interface IRabbitMQConsumerFactory
    {
        IConsumer Create(string componentName, string topic);
    }
}