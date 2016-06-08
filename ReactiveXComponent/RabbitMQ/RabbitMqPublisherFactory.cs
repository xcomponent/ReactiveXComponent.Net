using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMQ
{
    public class RabbitMqPublisherFactory : IRabbitMqPublisherFactory
    {
        private readonly IRabbitMqConnection _connection;

        public RabbitMqPublisherFactory(IRabbitMqConnection connection)
        {
            _connection = connection;
        }

        public IRabbitMqPublisher Create(string componentName)
        {
            return new RabbitMqPublisher(HashcodeHelper.GetXcHashCode(componentName).ToString(), _connection);       
        }
    }
}
