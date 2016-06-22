using ReactiveXComponent.Configuration;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent.Connection
{
    public class XCConnectionFactory : AbstractXCConnectionFactory
    {
        private readonly XCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        public XCConnectionFactory(XCConfiguration xcConfiguration, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public override IXCConnection CreateConnection()
        {
            return new RabbitMqConnection(_xcConfiguration, _privateCommunicationIdentifier);
        }
    }
}
