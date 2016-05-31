using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMQ
{
    public class RabbitMqPublisherFactory : IRabbitMqPublisherFactory
    {
        private readonly IRabbitMqConnection connection;

        public RabbitMqPublisherFactory(IRabbitMqConnection connection)
        {
            this.connection = connection;
        }

        public IRabbitMQPublisher Create(string componentName)
        {
            return new RabbitMqPublisher(HashcodeHelper.GetXcHashCode(componentName).ToString(), this.connection);       
        }
    }
}
