using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMQ
{
    using System.Globalization;

    public class SingleKeyRabbitMqConsumerFactory : IRabbitMqConsumerFactory
    {
        private readonly IRabbitMqConnection connection;

        public SingleKeyRabbitMqConsumerFactory(IRabbitMqConnection connection)
        {
            this.connection = connection;
        }

        public IConsumer Create(string componentName, string topic)
        {
            return new SingleKeyRabbitMqConsumer(HashcodeHelper.GetXcHashCode(componentName).ToString(CultureInfo.InvariantCulture), topic, this.connection);
        }
    }
}
