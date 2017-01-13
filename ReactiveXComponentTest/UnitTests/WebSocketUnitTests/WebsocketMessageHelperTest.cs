using System;
using System.IO;
using System.Text;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.WebSocket;

namespace ReactiveXComponentTest.UnitTests.WebSocketUnitTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class WebsocketMessageHelperTest
    {
        private int _helloworldComponent;
        private string _message;
        private string _topic;
        private WebSocketEngineHeader _webSocketEngineHeader;
        private WebSocketPacket _webSocketPacket;

        [SetUp]
        public void Setup()
        {   
            _helloworldComponent = -69981087;
            const long helloworldStateMachine = -829536631;

            _message = "HelloWorld";
            _topic = "HelloWorldTopic";
            _webSocketEngineHeader = new WebSocketEngineHeader
            {
                ComponentCode = new Option<long>(_helloworldComponent),
                StateMachineCode = new Option<long>(helloworldStateMachine),
                EventCode = 0,
                MessageType = new Option<string>(_message.GetType().ToString()),
                PublishTopic = null,
                AgentId = null,
                EngineCode = null,
                IncomingType = 0,
                Probes = null,
                SessionData = null,
                SessionId = null,
                StateCode = null,
                StateMachineId = null
            };
            _webSocketPacket = new WebSocketPacket
            {
                Header = _webSocketEngineHeader,
                JsonMessage = SerializeToString(_message),
            };
        }
        
        [TestCase(WebSocketCommand.Input)]
        [TestCase(WebSocketCommand.Subscribe)]
        [TestCase(WebSocketCommand.Unsubscribe)]
        [TestCase(WebSocketCommand.Snapshot)]
        [TestCase(WebSocketCommand.Error)]
        [TestCase(WebSocketCommand.Update)]
        [TestCase("")]
        public void SerializeRequest_SuccessfulSerialization_Test(string command)
        {
            var request = WebSocketMessageHelper.SerializeRequest(command, _webSocketEngineHeader, _message, 
                _helloworldComponent.ToString(), _topic);

            var header = SerializeToString(_webSocketPacket);
            var expectedRequest = $"{command} {_topic} {_helloworldComponent} {header}{Environment.NewLine}";

            Check.That(request.Equals(expectedRequest)).IsTrue();
        }

        [TestCase(WebSocketCommand.Input)]
        [TestCase(WebSocketCommand.Subscribe)]
        [TestCase(WebSocketCommand.Unsubscribe)]
        [TestCase(WebSocketCommand.Snapshot)]
        [TestCase(WebSocketCommand.Error)]
        [TestCase(WebSocketCommand.Update)]
        public void DeserializeRequest_SuccessfulDeserialization_Test(string command)
        {
            var json = SerializeToString(_webSocketPacket);
            var request = $"{command} {_topic} {_helloworldComponent} {json}{Environment.NewLine}";

            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(request);
            var expectedWebSocketMessage = new WebSocketMessage(command, _topic ,json, _helloworldComponent.ToString());

            Check.That(webSocketMessage).IsEqualTo(expectedWebSocketMessage);
        }

        [TestCase("")]
        public void DeserializeRequest_WithBlankAsCommand_SuccessfulDeserialization_Test(string command)
        {
            var json = SerializeToString(_webSocketPacket);
            var request = $"{command} {_topic} {_helloworldComponent} {json}{Environment.NewLine}";

            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(request);
            var expectedWebSocketMessage = new WebSocketMessage(WebSocketCommand.Input, _topic, json, _helloworldComponent.ToString());

            Check.That(webSocketMessage).IsEqualTo(expectedWebSocketMessage);
        }

        [Test]
        public void DeserializeRequest_WithoutCommand_SuccessfulDeserialization_Test()
        {
            var json = SerializeToString(_webSocketPacket);
            var request = $"{_topic} {_helloworldComponent} {json}{Environment.NewLine}";

            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(request);
            var expectedWebSocketMessage = new WebSocketMessage(WebSocketCommand.Input, _topic, json, _helloworldComponent.ToString());

            Check.That(webSocketMessage).IsEqualTo(expectedWebSocketMessage);
        }

        [TestCase(WebSocketCommand.Input)]
        [TestCase(WebSocketCommand.Subscribe)]
        [TestCase(WebSocketCommand.Unsubscribe)]
        [TestCase(WebSocketCommand.Snapshot)]
        [TestCase(WebSocketCommand.Error)]
        [TestCase(WebSocketCommand.Update)]
        public void DeserializeRequest_WithCommandAttachedByDotToTopic_SuccessfulDeserialization_Test(string command)
        {
            var json = SerializeToString(_webSocketPacket);
            var newTopic = $"{command}.{_topic}"; 
            var request = $"{newTopic} {_helloworldComponent} {json}{Environment.NewLine}";

            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(request);
            var expectedWebSocketMessage = new WebSocketMessage(command, newTopic, json, _helloworldComponent.ToString());

            Check.That(webSocketMessage).IsEqualTo(expectedWebSocketMessage);
        }

        [TestCase("")]
        public void DeserializeRequest_WithDotAttachedToTopic_SuccessfulDeserialization_Test(string command)
        {
            var json = SerializeToString(_webSocketPacket);
            var newTopic = $"{command}.{_topic}";
            var request = $"{newTopic} {_helloworldComponent} {json}{Environment.NewLine}";

            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(request);
            var expectedWebSocketMessage = new WebSocketMessage(WebSocketCommand.Input, _topic, json, _helloworldComponent.ToString());

            Check.That(webSocketMessage).IsEqualTo(expectedWebSocketMessage);
        }

        [Test]
        public void DeserializeRequest_WithoutCommandTopicNorComponentcode_ThrowsInvalidOperationException_Test()
        {
            var json = SerializeToString(_webSocketPacket);
            var request = $"{json}{Environment.NewLine}";

            Check.ThatCode(() => { WebSocketMessageHelper.DeserializeRequest(request); }).Throws<InvalidOperationException>();
        }

        [TestCase(WebSocketCommand.Input)]
        [TestCase(WebSocketCommand.Subscribe)]
        [TestCase(WebSocketCommand.Unsubscribe)]
        [TestCase(WebSocketCommand.Snapshot)]
        [TestCase(WebSocketCommand.Error)]
        [TestCase(WebSocketCommand.Update)]
        public void DeserializeRequest_WithCommandAttachedToTopicWithDots_ThrowsInvalidOperationException_Test(string command)
        {
            var json = SerializeToString(_webSocketPacket);
            var newTopic = $"{command}{_topic}.{_topic}";
            var request = $"{newTopic} {_helloworldComponent} {json}{Environment.NewLine}";

            Check.ThatCode(() => { WebSocketMessageHelper.DeserializeRequest(request); }).Throws<InvalidOperationException>();
        }


        private string SerializeToString(object message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new ReactiveXComponent.Serializer.JsonSerializer();
                serializer.Serialize(stream, message);
                stream.Flush();

                string serializedMessage = Encoding.UTF8.GetString(stream.ToArray());
                return serializedMessage;
            }
        }

        [TearDown]
        public void TearDown()
        {
            _webSocketEngineHeader = null;
            _webSocketPacket = null;
        }
    }
}
