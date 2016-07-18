using RabbitMQ.Client;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSession : IXCSession
    {
        private readonly IXCConfiguration _xcConfiguration;
        private readonly IConnection _connection;
        private readonly string _privateCommunicationIdentifier;
        private readonly SerializerFactory _serializerFactory;

        public RabbitMqSession(IXCConfiguration xcConfiguration, IConnection connection , string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializerFactory = InitSerializerFactory();
        }

        private SerializerFactory InitSerializerFactory()
        {
            switch (_xcConfiguration.GetSerializationType())
            {
                case XCApiTags.Binary:
                    return new SerializerFactory(SerializationType.Binary);
                case XCApiTags.Json:
                    return new SerializerFactory(SerializationType.Json);
                default:
                    throw new XCSerializationException("SerializerFactory init failed");
            }
        }

        public IXCPublisher CreatePublisher(string component)
        {
            return new RabbitMqPublisher(component, _xcConfiguration, _connection, _serializerFactory, _privateCommunicationIdentifier);
        }
   
        public IXCSubscriber CreateSubscriber()
        {
                return new RabbitMqSubscriber(_xcConfiguration, _connection, _serializerFactory, _privateCommunicationIdentifier);
        }
    }
}