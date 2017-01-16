using System;
using NFluent;
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
            
            var subscribeRequest = isPrivate ?
                "subscribe {\"Header\":{\"EventCode\":0,\"IncomingType\":0},\"JsonMessage\":\"{\\\"Topic\\\":{\\\"Key\\\":\\\"" + _privateTopic + "\\\",\\\"Kind\\\":2}}\"}" + Environment.NewLine
                : "subscribe {\"Header\":{\"EventCode\":0,\"IncomingType\":0},\"JsonMessage\":\"{\\\"Topic\\\":{\\\"Key\\\":\\\"" + _publicTopic + "\\\",\\\"Kind\\\":3}}\"}" + Environment.NewLine;

            Assert.AreEqual(webSocketRequest, subscribeRequest);
        }

        [Test]
        public void UpdateReceivedTest()
        {
            string rawRequest = "update output.1_0.HelloWorldMicroservice.GoodByeWorld.Result -824151934 {\"Header\":{\"StateCode\":{\"Case\":\"Some\",\"Fields\":[0]},\"StateMachineId\":{\"Case\":\"Some\",\"Fields\":[81]},\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},\"ComponentCode\":{\"Case\":\"Some\",\"Fields\":[-824151934]},\"EventCode\":0,\"Probes\":[],\"IncomingType\":9,\"AgentId\":{\"Case\":\"Some\",\"Fields\":[0]},\"MessageType\":{\"Case\":\"Some\",\"Fields\":[\"XComponent.GoodByeWorld.UserObject.Result\"]}},\"JsonMessage\":\"{\\\"Value\\\":\\\"GoodBye\\\"}\"}";
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
    }
}
