using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponent.WebSocket;

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

        public override IXCConnection CreateConnection(ConnectionType connectionType, int connectionTimeout = 10000)
        {
            switch (connectionType)
            {
                case ConnectionType.RabbitMq:
                    return new RabbitMqConnection(_xcConfiguration, _privateCommunicationIdentifier);
                case ConnectionType.WebSocket:
                    var webSocketEndpoint = _xcConfiguration.GetWebSocketEndpoint();
                    return new WebSocketConnection(_xcConfiguration, webSocketEndpoint, _privateCommunicationIdentifier);
                default:
                    throw new ReactiveXComponentException($"Unsupported connection type: {connectionType}");
            }
        }
    }
}
