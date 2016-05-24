namespace XCClientLib.RabbitMQ
{
    using System.Globalization;

    using XComponent.Common.Helper;

    public class SingleKeyRabbitMQConsumerFactory : IRabbitMQConsumerFactory
    {
        private readonly IRabbitMqConnection connection;

        public SingleKeyRabbitMQConsumerFactory(IRabbitMqConnection connection)
        {
            this.connection = connection;
        }

        public IConsumer Create(string componentName, string topic)
        {
            return new SingleKeyRabbitMqConsumer(HashcodeHelper.GetXcHashCode(componentName).ToString(CultureInfo.InvariantCulture), topic, this.connection);
        }
    }
}
