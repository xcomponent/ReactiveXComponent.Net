using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.Configuration
{
    [TestFixture]
    public class ConfigurationTests
    {
        private const string RabbitMqApiFileName = "RabbitMqTestApi.xcApi";
        private const string WebSocketApiFileName = "webSocketTestApi.xcApi";

        private IXCConfiguration _rabbitMqConfiguration;
        private IXCConfiguration _webSocketConfiguration;

        private string GetApiFileName(ConnectionType connectionType)
        {
            var apiFileName = string.Empty;

            if (connectionType == ConnectionType.RabbitMq)
            {
                apiFileName = RabbitMqApiFileName;
            }
            else if (connectionType == ConnectionType.WebSocket)
            {
                apiFileName = WebSocketApiFileName;
            }

            return apiFileName;
        }

        [SetUp]
        public void SetUp()
        {
            var rabbitMqstream = new FileStream(RabbitMqApiFileName, FileMode.Open, FileAccess.Read);
            _rabbitMqConfiguration = new XCConfiguration(new XCApiConfigParser());
            _rabbitMqConfiguration.Init(rabbitMqstream);

            var webSocketstream = new FileStream(WebSocketApiFileName, FileMode.Open, FileAccess.Read);
            _webSocketConfiguration = new XCConfiguration(new XCApiConfigParser());
            _webSocketConfiguration.Init(webSocketstream);
        }

        [TestCase(ConnectionType.RabbitMq)]
        [TestCase(ConnectionType.WebSocket)]
        public void GetConnectionTypeTest(ConnectionType connectionType)
        {
            var apiFileName = GetApiFileName(connectionType);
            
            if(string.IsNullOrEmpty(apiFileName))
            {
                Assert.Fail($"Unknown connection type: {connectionType}");
                return;
            }

            using (var stream = new FileStream(apiFileName, FileMode.Open, FileAccess.Read))
            {
                var xcConfiguration = new XCConfiguration(new XCApiConfigParser());
                xcConfiguration.Init(stream);

                var parsedConnectionType = xcConfiguration.GetConnectionType();

                Check.That(parsedConnectionType).IsEqualTo(connectionType);
            }
        }

        [Test]
        public void GetBusDetailsTest()
        {
            var busDetails = _rabbitMqConfiguration.GetBusDetails();

            Check.That(busDetails.Host).IsEqualTo("127.0.0.1");
            Check.That(busDetails.Username).IsEqualTo("guest");
            Check.That(busDetails.Password).IsEqualTo("guest");
            Check.That(busDetails.Port).IsEqualTo(5672);
        }

        [Test]
        public void GetWebSocketEndpointTest()
        {
            var webSocketEndpoint = _webSocketConfiguration.GetWebSocketEndpoint();

            Check.That(webSocketEndpoint.Name).IsEqualTo("websocket");
            Check.That(webSocketEndpoint.Host).IsEqualTo("localhost");
            Check.That(webSocketEndpoint.Port).IsEqualTo("443");
            Check.That(webSocketEndpoint.Type).IsEqualTo(WebSocketType.Secure);
        }

        [Test]
        public void GetStateMachineCodeTest()
        {
            var stateMachineCode = _webSocketConfiguration.GetStateMachineCode("HelloWorld", "HelloWorldManager");

            Check.That(stateMachineCode).IsEqualTo(-829536631);
        }

        [Test]
        public void GetComponentCodeTest()
        {
            var componentCode = _webSocketConfiguration.GetComponentCode("HelloWorld");

            Check.That(componentCode).IsEqualTo(-69981087);
        }

        [Test]
        public void GetPublisherEventCodeTest()
        {
            var eventCode = _webSocketConfiguration.GetPublisherEventCode("XComponent.HelloWorld.UserObject.SayHello");

            Check.That(eventCode).IsEqualTo(9);
        }

        [Test]
        public void GetPublisherTopicFromNamesTest()
        {
            var publisherTopic = _webSocketConfiguration.GetPublisherTopic("HelloWorld", "HelloWorldManager");

            Check.That(publisherTopic).IsEqualTo("input.1_0.HelloMicroservice.HelloWorld.HelloWorldManager");
        }

        [Test]
        public void GetPublisherTopicFromCodesTest()
        {
            var publisherTopic = _webSocketConfiguration.GetPublisherTopic(-69981087, -829536631);

            Check.That(publisherTopic).IsEqualTo("input.1_0.HelloMicroservice.HelloWorld.HelloWorldManager");
        }

        [Test]
        public void GetSubscriberTopicTest()
        {
            var subscriberTopic = _webSocketConfiguration.GetSubscriberTopic("HelloWorld", "HelloResponse");

            Check.That(subscriberTopic).IsEqualTo("output.1_0.HelloMicroservice.HelloWorld.HelloResponse");
        }

        [Test]
        public void GetSerializationTypeTest()
        {
            var serializationType = _rabbitMqConfiguration.GetSerializationType();

            Check.That(serializationType).IsEqualTo("Binary");
        }

        [Test]
        public void GetSnapshotTopicTest()
        {
            var snapshotTopic = _webSocketConfiguration.GetSnapshotTopic("HelloWorld");

            Check.That(snapshotTopic).IsEqualTo("snapshot.1_0.HelloMicroservice.HelloWorld");
        }
    }
}
