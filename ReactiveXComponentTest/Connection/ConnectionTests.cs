using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponent.WebSocket;

namespace ReactiveXComponentTest.Connection
{
    [TestFixture]
    public class ConnectionTests
    {
        private string _privateTopic = "d5c59d2b-58c9-4a1d-b305-150accfe6cd6";
        private XCConfiguration _rabbitMqConfiguration;
        private XCConfiguration _webSocketConfiguration;

        [SetUp]
        public void SetUp()
        {
            var parser = Substitute.For<IXCApiConfigParser>();
            
            _rabbitMqConfiguration = Substitute.For<XCConfiguration>(parser);
            _webSocketConfiguration = Substitute.For<XCConfiguration>(parser);
            
            var rabbitBusDetails = new BusDetails("guest", "guest", "localhost", 5672);
            var webSocketEndpoint = new WebSocketEndpoint("websocket", "localhost", "443", WebSocketType.Secure);

            _rabbitMqConfiguration.GetBusDetails().Returns(rabbitBusDetails);
            _rabbitMqConfiguration.GetWebSocketEndpoint().Returns(webSocketEndpoint);
        }

        [TestCase(ConnectionType.RabbitMq)]
        [TestCase(ConnectionType.WebSocket)]
        public void ConnectionFactoryTest(ConnectionType connectionType)
        {
            AbstractXCConnectionFactory connectionFactory = null;
            IXCConnection connection = null;
            switch (connectionType)
            {
                case ConnectionType.RabbitMq:
                    connectionFactory = new XCConnectionFactory(_rabbitMqConfiguration, _privateTopic);
                    connection = connectionFactory.CreateConnection(connectionType);
                    Check.That(connection).IsInstanceOf<RabbitMqConnection>();
                    break;
                case ConnectionType.WebSocket:
                    connectionFactory = new XCConnectionFactory(_webSocketConfiguration, _privateTopic);
                    connection = connectionFactory.CreateConnection(connectionType);
                    Check.That(connection).IsInstanceOf<WebSocketConnection>();
                    break;
                default:
                    Assert.Fail($"Unknown connection type: {connectionType.ToString()}");
                    break;
            }
        }
    }
}
