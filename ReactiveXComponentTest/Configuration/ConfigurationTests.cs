using System;
using System.IO;
using System.Security.Authentication;
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
        private readonly string _rabbitMqApiFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RabbitMqTestApi.xcApi");
        private readonly string _webSocketApiFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebSocketTestApi.xcApi");

        private IXCConfiguration _rabbitMqConfiguration;
        private IXCConfiguration _webSocketConfiguration;

        private string GetApiFileName(ConnectionType connectionType)
        {
            var apiFileName = string.Empty;

            if (connectionType == ConnectionType.RabbitMq)
            {
                apiFileName = _rabbitMqApiFileName;
            }
            else if (connectionType == ConnectionType.WebSocket)
            {
                apiFileName = _webSocketApiFileName;
            }

            return apiFileName;
        }

        [SetUp]
        public void SetUp()
        {
            var rabbitMqstream = new FileStream(_rabbitMqApiFileName, FileMode.Open, FileAccess.Read);
            _rabbitMqConfiguration = new XCConfiguration(new XCApiConfigParser());
            _rabbitMqConfiguration.Init(rabbitMqstream);

            var webSocketstream = new FileStream(_webSocketApiFileName, FileMode.Open, FileAccess.Read);
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
            Check.That(busDetails.VirtualHost).IsEqualTo("myVirtualHost");
            Check.That(busDetails.Username).IsEqualTo("guest");
            Check.That(busDetails.Password).IsEqualTo("guest");
            Check.That(busDetails.Port).IsEqualTo(5671);
            Check.That(busDetails.SslEnabled).IsTrue();
            Check.That(busDetails.SslServerName).IsEqualTo("XComponent RMq");
            Check.That(busDetails.SslCertificatePath).IsEqualTo("some_cert_path");
            Check.That(busDetails.SslCertificatePassphrase).IsEqualTo("some_cert_pass");
            Check.That(busDetails.SslProtocol).IsEqualTo(SslProtocols.Tls12);
            Check.That(busDetails.SslAllowUntrustedServerCertificate).IsTrue();
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
        public void GetStateCodeTest()
        {
            var stateCode = _webSocketConfiguration.GetStateCode("HelloWorld", "HelloWorldManager", "EntryPoint");

            Check.That(stateCode).IsEqualTo(0);
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
