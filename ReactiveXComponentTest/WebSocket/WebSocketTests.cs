using System;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.WebSocket;

namespace ReactiveXComponentTest.WebSocket
{
    [TestFixture]
    public class WebSocketTests
    {
        private long _componentCode;
        private long _stateMachineCode;
        private string _publicTopic;
        private string _privateTopic;

        [SetUp]
        public void SetUp()
        {
            _componentCode = -824151934;
            _stateMachineCode = 405360011;
            _publicTopic = "input.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager";
            _privateTopic = "d5c59d2b-58c9-4a1d-b305-150accfe6cd6";
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SubscriptionTest(bool isPrivate)
        {
            var webSocketTopic = isPrivate ? WebSocketTopic.Private(_privateTopic) : WebSocketTopic.Public(_publicTopic);
            var webSocketSubscription = new WebSocketSubscription(webSocketTopic);
            var subscriptionHeader = new WebSocketEngineHeader();
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                WebSocketCommand.Subscribe,
                subscriptionHeader,
                webSocketSubscription);

            var subscribeRequest = isPrivate
                ? "subscribe {\"Header\":{\"EventCode\":0,\"IncomingType\":0},\"JsonMessage\":\"{\\\"Topic\\\":{\\\"Key\\\":\\\"" +
                  _privateTopic + "\\\",\\\"Kind\\\":2}}\"}" + Environment.NewLine
                : "subscribe {\"Header\":{\"EventCode\":0,\"IncomingType\":0},\"JsonMessage\":\"{\\\"Topic\\\":{\\\"Key\\\":\\\"" +
                  _publicTopic + "\\\",\\\"Kind\\\":3}}\"}" + Environment.NewLine;

            Assert.AreEqual(webSocketRequest, subscribeRequest);
        }

        [Test]
        public void UpdateReceivedTest()
        {
            string rawRequest =
                "update output.1_0.HelloWorldMicroservice.GoodByeWorld.Result -824151934 {\"Header\":{\"StateCode\":{\"Case\":\"Some\",\"Fields\":[0]},\"StateMachineId\":{\"Case\":\"Some\",\"Fields\":[81]},\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},\"ComponentCode\":{\"Case\":\"Some\",\"Fields\":[-824151934]},\"EventCode\":0,\"Probes\":[],\"IncomingType\":9,\"AgentId\":{\"Case\":\"Some\",\"Fields\":[0]},\"MessageType\":{\"Case\":\"Some\",\"Fields\":[\"XComponent.GoodByeWorld.UserObject.Result\"]}},\"JsonMessage\":\"{\\\"Value\\\":\\\"GoodBye\\\"}\"}";
            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(rawRequest);

            Assert.IsNotNullOrEmpty(webSocketMessage.Command);
            Assert.AreEqual(webSocketMessage.Command, WebSocketCommand.Update);
            Assert.IsNotNullOrEmpty(webSocketMessage.Topic);
            Assert.IsNotNullOrEmpty(webSocketMessage.ComponentCode);
            Assert.AreEqual(webSocketMessage.ComponentCode, _componentCode.ToString());

            var webSocketPacket = WebSocketMessageHelper.DeserializePacket(webSocketMessage);

            Assert.AreEqual(webSocketPacket.Header.ComponentCode.Fields[0], _componentCode);
            Assert.AreEqual(webSocketPacket.Header.StateMachineCode.Fields[0], _stateMachineCode);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SendMessageTest(bool isPrivate)
        {
            var message = "Hello";
            var messageType = message?.GetType();
            var inputHeader = new WebSocketEngineHeader
            {
                ComponentCode = new Option<long>(_componentCode),
                StateMachineCode = new Option<long>(_stateMachineCode),
                EventCode = 9,
                MessageType = new Option<string>(messageType?.ToString()),
                PublishTopic = isPrivate ? new Option<string>(_privateTopic) : null
            };
            var topic = _publicTopic;
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                WebSocketCommand.Input,
                inputHeader,
                message,
                _componentCode.ToString(),
                topic);

            var publishRequest = isPrivate
                ? "input input.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager -824151934 {\"Header\":{\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},"
                  +"\"ComponentCode\":{\"Case\":\"Some\",\"Fields\":[-824151934]},\"EventCode\":9,\"IncomingType\":0,\"MessageType\":{\"Case\":\"Some\",\"Fields\":"
                  +"[\"System.String\"]},\"PublishTopic\":{\"Case\":\"Some\",\"Fields\":[\"" + _privateTopic + "\"]}},\"JsonMessage\":\"\\\"Hello\\\"\"}" + Environment.NewLine
                : "input input.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager -824151934 {\"Header\":{\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},"
                  +"\"ComponentCode\":{\"Case\":\"Some\",\"Fields\":[-824151934]},\"EventCode\":9,\"IncomingType\":0,\"MessageType\":{\"Case\":\"Some\",\"Fields\":" 
                  +"[\"System.String\"]}},\"JsonMessage\":\"\\\"Hello\\\"\"}" + Environment.NewLine;

            Assert.AreEqual(webSocketRequest, publishRequest);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SendMessageWithStateMachineRefTest(bool isPrivate)
        {
            var message = "Hello";
            var messageType = message?.GetType();
            var stateMachineRef = new StateMachineRefHeader()
            {
                AgentId = 0,
                StateMachineId = 0,
                ComponentCode = _componentCode,
                StateMachineCode = _stateMachineCode,
                StateCode = 0,
                EventCode = 9,
                PublishTopic = isPrivate ? _privateTopic : null
            };
            var inputHeader = new WebSocketEngineHeader
            {
                AgentId = new Option<int>(stateMachineRef.AgentId),
                StateMachineId = new Option<long>(stateMachineRef.StateMachineId),
                ComponentCode = new Option<long>(stateMachineRef.ComponentCode),
                StateMachineCode = new Option<long>(stateMachineRef.StateMachineCode),
                StateCode = new Option<int>(stateMachineRef.StateCode),
                EventCode = stateMachineRef.EventCode,
                MessageType = new Option<string>(messageType?.ToString()),
                PublishTopic = new Option<string>(stateMachineRef.PublishTopic)
            };
            var topic = _publicTopic;
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                WebSocketCommand.Input,
                inputHeader,
                message,
                _componentCode.ToString(),
                topic);

            var publishRequest = isPrivate
                ? "input input.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager -824151934 {\"Header\":{\"StateMachineId\":{\"Case\":\"Some\",\"Fields\":[0]}," 
                 +"\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},\"ComponentCode\":{\"Case\":\"Some\",\"Fields\":[-824151934]}," 
                 +"\"StateCode\":{\"Case\":\"Some\",\"Fields\":[0]},\"EventCode\":9,\"IncomingType\":0,\"AgentId\":{\"Case\":\"Some\",\"Fields\":[0]}," 
                 +"\"MessageType\":{\"Case\":\"Some\",\"Fields\":[\"System.String\"]},\"PublishTopic\":{\"Case\":\"Some\",\"Fields\":[\"" 
                 +_privateTopic + "\"]}},\"JsonMessage\":\"\\\"Hello\\\"\"}" + Environment.NewLine
                : "input input.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager -824151934 {\"Header\":{\"StateMachineId\":{\"Case\":\"Some\",\"Fields\":[0]}," 
                 +"\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},\"ComponentCode\":{\"Case\":\"Some\",\"Fields\":[-824151934]}," 
                 +"\"StateCode\":{\"Case\":\"Some\",\"Fields\":[0]},\"EventCode\":9,\"IncomingType\":0,\"AgentId\":{\"Case\":\"Some\",\"Fields\":[0]}," 
                 +"\"MessageType\":{\"Case\":\"Some\",\"Fields\":[\"System.String\"]}},\"JsonMessage\":\"\\\"Hello\\\"\"}" + Environment.NewLine;

            Assert.AreEqual(webSocketRequest, publishRequest);
        }


        [TestCase(true)]
        [TestCase(false)]
        public void SendSnapshotRequest(bool isPrivate)
        {
            var inputHeader = new WebSocketEngineHeader();
            var componentCode = _componentCode;
            var topic = "snapshot.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager";
            var replyTopic = "replyTopic";
            var stateMachineCode = _stateMachineCode;
            var snapshotMessage = isPrivate? new WebSocketSnapshotMessage(stateMachineCode, componentCode, replyTopic, _privateTopic): new WebSocketSnapshotMessage(stateMachineCode, componentCode, replyTopic, null);
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                    WebSocketCommand.Snapshot,
                    inputHeader,
                    snapshotMessage,
                    componentCode.ToString(),
                    topic);

            var snaphsotRequest = isPrivate
                ? "snapshot snapshot.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager -824151934 {\"Header\":{\"EventCode\":0,\"IncomingType\":0},"
                  + "\"JsonMessage\":\"{\\\"StateMachineCode\\\":405360011,\\\"ComponentCode\\\":-824151934,\\\"ReplyTopic\\\":{\\\"Case\\\":\\\"Some\\\",\\\"Fields\\\":[\\\"replyTopic\\\"]},"
                  + "\\\"PrivateTopic\\\":{\\\"Case\\\":\\\"Some\\\",\\\"Fields\\\":[[\\\"" + _privateTopic 
                  +"\\\"]]}}\"}" + Environment.NewLine
                : "snapshot snapshot.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager -824151934 {\"Header\":{\"EventCode\":0,\"IncomingType\":0},"
                  + "\"JsonMessage\":\"{\\\"StateMachineCode\\\":405360011,\\\"ComponentCode\\\":-824151934,\\\"ReplyTopic\\\":{\\\"Case\\\":\\\"Some\\\",\\\"Fields\\\":[\\\"replyTopic\\\"]},"
                  + "\\\"PrivateTopic\\\":{\\\"Case\\\":\\\"Some\\\",\\\"Fields\\\":[[null]]}}\"}" + Environment.NewLine;

            Assert.AreEqual(webSocketRequest, snaphsotRequest);
        }
    }
}
