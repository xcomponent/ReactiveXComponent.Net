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
        private readonly ISerializer _serializer;

        public RabbitMqSession(IXCConfiguration xcConfiguration, IConnection connection , string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializer = SelectSerializer();
        }

        private ISerializer SelectSerializer()
        {
            switch (_xcConfiguration.GetSerializationType())
            {
                case XCApiTags.Binary:
                    return SerializerFactory.CreateSerializer(SerializationType.Binary);
                case XCApiTags.Json:
                    return SerializerFactory.CreateSerializer(SerializationType.Json);
                default:
                    throw new XCSerializationException("Serialization type not supported");
            }
        }

        public IXCPublisher CreatePublisher(string component)
        {
            return new RabbitMqPublisher(component, _xcConfiguration, _connection, _serializer, _privateCommunicationIdentifier);
        }
   
        public IXCSubscriber CreateSubscriber()
        {
                return new RabbitMqSubscriber(_xcConfiguration, _connection, _serializer, _privateCommunicationIdentifier);
        }
    }
}