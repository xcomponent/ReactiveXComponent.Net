using RabbitMQ.Client;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSession : IXCSession
    {
        private readonly IXCConfiguration _xcConfiguration;
        private readonly IConnection _connection;

        public RabbitMqSession(IXCConfiguration xcConfiguration, IConnection connection)
        {
            _xcConfiguration = xcConfiguration;
            _connection = connection;
        }

        public IXCPublisher CreatePublisher(string component)
        {
            return new RabbitMqPublisher(component, _xcConfiguration, _connection);
        }
    }
}