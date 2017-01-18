namespace XCClientLib.RabbitMQ
{
    public interface IRabbitMqPublisherFactory
    {
        IRabbitMQPublisher Create(string componentName);
    }
}