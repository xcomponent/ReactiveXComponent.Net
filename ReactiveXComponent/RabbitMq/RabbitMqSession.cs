using RabbitMQ.Client;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSession : IXCSession
    {
        private readonly IXCConfiguration _xcConfiguration;
        private readonly IConnection _connection;
        private readonly string _privateCommunicationIdentifier;

        public RabbitMqSession(IXCConfiguration xcConfiguration, IConnection connection , string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public IXCPublisher CreatePublisher(string component)
        {
            return new RabbitMqPublisher(component, _xcConfiguration, _connection, _privateCommunicationIdentifier);
        }
    }
}