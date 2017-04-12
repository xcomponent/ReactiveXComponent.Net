using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.WebSocket;

namespace ReactiveXComponentTest.WebSocket
{
    [TestFixture]
    public class WebSocketTests
    {
        private const string PrivateTopic = "d5c59d2b-58c9-4a1d-b305-150accfe6cd6";

        [Test]
        public void SubscriberTest()
        {
            var millisecondsTimeout = 10000;
            var componentName = "GoodByeWorld";
            var componentCode = -824151934;
            var stateMachineName = "Result";
            var stateMachineCode = 405360011;
            var subscriberPublicTopic = "output.1_0.HelloWorldMicroservice.GoodByeWorld.Result";
            var snapshotTopic = "snapshot.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager";

            var xcConfiguration = Substitute.For<IXCConfiguration>();
            xcConfiguration.GetComponentCode(componentName).Returns(x => componentCode);
            xcConfiguration.GetStateMachineCode(componentName, stateMachineName).Returns(x => stateMachineCode);
            xcConfiguration.GetSubscriberTopic(componentName, stateMachineName).Returns(x => subscriberPublicTopic);
            xcConfiguration.GetSnapshotTopic(componentName).Returns(x => snapshotTopic);

            var webSocketClient = Substitute.For<IWebSocketClient>();
            webSocketClient.IsOpen.Returns(true);
            webSocketClient.When(x => x.Open()).DoNotCallBase();
            webSocketClient.When(x => x.Close()).DoNotCallBase();
            webSocketClient.When(x => x.Dispose()).DoNotCallBase();
            webSocketClient.WhenForAnyArgs(x => x.Send("")).DoNotCallBase();

            using (var webSocketSubscriber = new WebSocketSubscriber(componentName, webSocketClient, xcConfiguration, PrivateTopic))
            using (var messageReceivedEvent = new AutoResetEvent(false))
            using (var messageReceivedInStreamEvent = new AutoResetEvent(false))
            {
                var handler = new Action<MessageEventArgs>(args =>
                {
                    if (args.StateMachineRefHeader.ComponentCode == componentCode
                        && args.StateMachineRefHeader.StateMachineCode == stateMachineCode)
                    {
                        messageReceivedEvent.Set();
                    }
                });

                webSocketSubscriber.Subscribe(stateMachineName, handler);

                var updatesObserver = Observer.Create<MessageEventArgs>(args => {
                    if (args.StateMachineRefHeader.ComponentCode == componentCode
                        && args.StateMachineRefHeader.StateMachineCode == stateMachineCode)
                    {
                        messageReceivedInStreamEvent.Set();
                    }
                });

                var subscription = webSocketSubscriber.StateMachineUpdatesStream.Subscribe(updatesObserver);

                var data =
                    "update " + subscriberPublicTopic + " -824151934 {\"Header\":{\"StateCode\":{\"Case\":\"Some\",\"Fields\":[0]},\"StateMachineId\":{\"Case\":\"Some\",\"Fields\":[81]},\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},\"ComponentCode\":{\"Case\":\"Some\",\"Fields\":[-824151934]},\"EventCode\":0,\"Probes\":[],\"IncomingType\":9,\"AgentId\":{\"Case\":\"Some\",\"Fields\":[0]},\"MessageType\":{\"Case\":\"Some\",\"Fields\":[\"XComponent.GoodByeWorld.UserObject.Result\"]}},\"JsonMessage\":\"{\\\"Value\\\":\\\"GoodBye\\\"}\"}";
                var rawData = Encoding.UTF8.GetBytes(data);
                var messageEventArgs = new WebSocketMessageEventArgs(data, rawData);
                webSocketClient.MessageReceived += Raise.EventWith(messageEventArgs);

                var messageReceived = messageReceivedEvent.WaitOne(millisecondsTimeout);
                var messageReceivedInStream = messageReceivedInStreamEvent.WaitOne(millisecondsTimeout);

                // Make sure that subscription works
                Check.That(messageReceived).IsTrue();
                Check.That(messageReceivedInStream).IsTrue();

                subscription.Dispose();

                webSocketSubscriber.Unsubscribe(stateMachineName, handler);
                webSocketClient.MessageReceived += Raise.EventWith(messageEventArgs);

                messageReceived = messageReceivedEvent.WaitOne(millisecondsTimeout / 2);

                // Make sure that we are unsubscribed
                Check.That(messageReceived).IsFalse();
            }
        }

        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void PublisherSendMessageTest(bool isPrivate, bool withStateMachineRef)
        {
            var componentName = "HelloWorld";
            var componentCode = -824151934;
            var stateMachineName = "HelloWorldManager";
            var stateMachineCode = 405360011;
            var publisherTopic = "input.1_0.HelloWorldMicroservice.HelloWorld.HelloWorldManager";

            var stateMachineRef = new StateMachineRefHeader() {
                AgentId = 0,
                StateMachineId = 0,
                ComponentCode = componentCode,
                StateMachineCode = stateMachineCode,
                StateCode = 0,
                EventCode = 9,
                PublishTopic = isPrivate ? PrivateTopic : null
            };

            var messageToSend = "Hello";
            WebSocketMessage webSocketMessage = null;
            var messageSentEvent = new AutoResetEvent(false);

            var xcConfiguration = Substitute.For<IXCConfiguration>();
            xcConfiguration.GetComponentCode(componentName).Returns(x => componentCode);
            xcConfiguration.GetStateMachineCode(componentName, stateMachineName).Returns(x => stateMachineCode);
            xcConfiguration.GetPublisherTopic(componentName, stateMachineName).Returns(x => publisherTopic);
            xcConfiguration.GetPublisherEventCode("System.String").Returns(9);

            var webSocketClient = Substitute.For<IWebSocketClient>();
            webSocketClient.IsOpen.Returns(true);
            webSocketClient.When(x => x.Open()).DoNotCallBase();
            webSocketClient.When(x => x.Close()).DoNotCallBase();
            webSocketClient.When(x => x.Dispose()).DoNotCallBase();
            webSocketClient.WhenForAnyArgs(x => x.Send("")).Do(callInfo => 
            {
                var message = (string)callInfo.Args()[0];
                webSocketMessage = WebSocketMessageHelper.DeserializeRequest(message);
                
                messageSentEvent.Set();
            });

            using (var webSocketPublisher = new WebSocketPublisher(componentName, webSocketClient, xcConfiguration, PrivateTopic))
            {
                if (withStateMachineRef)
                {
                    webSocketPublisher.SendEvent(stateMachineRef, messageToSend, isPrivate ? Visibility.Private : Visibility.Public);
                }
                else
                {
                    webSocketPublisher.SendEvent(stateMachineName, messageToSend, isPrivate ? Visibility.Private : Visibility.Public);
                }

                var messageWasSent = messageSentEvent.WaitOne(10000);

                Check.That(messageWasSent).IsTrue();
                Check.That(webSocketMessage).IsNotNull();

                Check.That(webSocketMessage.Command).IsEqualTo(WebSocketCommand.Input);

                if (!withStateMachineRef)
                {
                    Check.That(webSocketMessage.Topic).IsEqualTo(publisherTopic);
                }

                Check.That(int.Parse(webSocketMessage.ComponentCode)).IsEqualTo(componentCode);

                var webSocketPacket = WebSocketMessageHelper.DeserializePacket(webSocketMessage);

                if (isPrivate)
                {
                    Check.That(webSocketPacket.Header.PublishTopic.Fields[0]).IsEqualTo(PrivateTopic);
                }
                else
                {
                    Check.That(webSocketPacket.Header.PublishTopic).IsNull();
                }

                if (withStateMachineRef)
                {
                    Check.That(webSocketPacket.Header.ComponentCode.Fields[0]).IsEqualTo(stateMachineRef.ComponentCode);
                    Check.That(webSocketPacket.Header.StateMachineCode.Fields[0]).IsEqualTo(stateMachineRef.StateMachineCode);
                    Check.That(webSocketPacket.Header.EventCode).IsEqualTo(stateMachineRef.EventCode);
                }

                var sentMessage = (string)JsonConvert.DeserializeObject(webSocketPacket.JsonMessage);
                Check.That(sentMessage).IsNotNull();
                Check.That(sentMessage).IsNotEmpty();
                Check.That(sentMessage).IsEqualTo(messageToSend);
            }
        }

        [Test]
        public void SnapshotTest()
        {
            var componentName = "HelloWorld";
            var componentCode = -69981087;
            var stateMachineName = "HelloResponse";
            var stateMachineCode = 1837059171;
            var publisherTopic = "input.1_0.MyHelloWorldService.HelloWorld.HelloResponse";
            var privateTopic = "d5c59d2b-58c9-4a1d-b305-150accfe6cd6";
            var snapshotTopic = "snapshot.1_0.MyHelloWorldService.HelloWorld";

            var xcConfiguration = Substitute.For<IXCConfiguration>();
            xcConfiguration.GetComponentCode(componentName).Returns(x => componentCode);
            xcConfiguration.GetStateMachineCode(componentName, stateMachineName).Returns(x => stateMachineCode);
            xcConfiguration.GetPublisherTopic(componentName, stateMachineName).Returns(x => publisherTopic);
            xcConfiguration.GetSnapshotTopic(componentName).Returns(x => snapshotTopic);

            var snapshotReplyTopic = string.Empty;
            var webSocketClient = Substitute.For<IWebSocketClient>();
            webSocketClient.IsOpen.Returns(true);
            webSocketClient.When(x => x.Open()).DoNotCallBase();
            webSocketClient.When(x => x.Close()).DoNotCallBase();
            webSocketClient.When(x => x.Dispose()).DoNotCallBase();
            webSocketClient.WhenForAnyArgs(x => x.Send("")).Do(callInfo =>
            {
                var message = (string)callInfo.Args()[0];
                var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(message);
                if (webSocketMessage.Command == WebSocketCommand.Snapshot)
                {
                    var snapshotPacket = JsonConvert.DeserializeObject<WebSocketPacket>(webSocketMessage.Json);
                    var snapshotMessage = JsonConvert.DeserializeObject<WebSocketSnapshotMessage>(snapshotPacket.JsonMessage);
                    snapshotReplyTopic = snapshotMessage.ReplyTopic.Fields[0];
                }
            });

            
            
            using (var webSocketPublisher = new WebSocketPublisher(componentName, webSocketClient, xcConfiguration, privateTopic))
            using (var snapshotReceivedEvent = new AutoResetEvent(false))
            {
                var millisecondsTimeout = 10000;
                var snapshotHandler = new Action<List<MessageEventArgs>>(args =>
                {
                    if (args.All(instance => instance.StateMachineRefHeader.ComponentCode == componentCode
                                    && instance.StateMachineRefHeader.StateMachineCode == stateMachineCode))
                    {
                        snapshotReceivedEvent.Set();
                    }
                });

                webSocketPublisher.GetSnapshotAsync(stateMachineName, snapshotHandler);

                var data = "snapshot " + snapshotReplyTopic + " -69981087 {\"Header\":{\"EventCode\":0,\"Probes\":[],\"IncomingType\":0,\"MessageType\":{\"Case\":\"Some\",\"Fields\":[\"XComponent.Common.Processing.SnapshotResponse\"]}},\"JsonMessage\":\"{\\\"Items\\\":\\\"H4sIAAAAAAAEAIuuVgouSSxJ9U1MzsjMS/VMUbIyMdBBEXPOT0lVsjK0MDY3MLU0NDeEykKFdZSc83ML8vNS80ogIrpmlpYWhgYW5jpKAaVJOZnJvqm5SalFSlbVSiH5SlZKjnn5pYlFSjpKQanFQH3FQC1KHpkKUOFaHSXHdKBZIIcY1sYCAAstn/OfAAAA\\\"}\"}";
                var rawData = Encoding.UTF8.GetBytes(data);
                var messageEventArgs = new WebSocketMessageEventArgs(data, rawData);
                webSocketClient.MessageReceived += Raise.EventWith(messageEventArgs);

                var snapshotReceived = snapshotReceivedEvent.WaitOne(millisecondsTimeout);
                Check.That(snapshotReceived).IsTrue();
            }
        }

        [Test]
        public void GetXCApiTest()
        {
            var expectedRequestProperty = string.Empty;
            var requestSentEvent = new AutoResetEvent(false);
            var webSocketClient = Substitute.For<IWebSocketClient>();
            webSocketClient.IsOpen.Returns(true);
            webSocketClient.When(x => x.Open()).DoNotCallBase();
            webSocketClient.When(x => x.Close()).DoNotCallBase();
            webSocketClient.When(x => x.Dispose()).DoNotCallBase();
            webSocketClient.WhenForAnyArgs(x => x.Send("")).Do(callInfo =>
            {
                var message = (string)callInfo.Args()[0];
                var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(message);
                if (webSocketMessage.Command == WebSocketCommand.GetXCApi)
                {
                    var xcApiProperties = JsonConvert.DeserializeObject<XCApiProperties>(webSocketMessage.Json);
                    expectedRequestProperty = xcApiProperties.Name;
                    requestSentEvent.Set();
                }
            });

            var xcApiManager = new WebSocketXCApiManager(webSocketClient);
            xcApiManager.GetXCApi("HelloWorld.api");
            var requestWasSent = requestSentEvent.WaitOne(TimeSpan.FromSeconds(5));

            Check.That(requestWasSent).IsTrue();
            Check.That(expectedRequestProperty.Equals("HelloWorld.api")).IsTrue();
        }

        [Test]
        public void GetXCApiListTest()
        {
            List<string> ExpectedXCApiList = null;
            var requestSentEvent = new AutoResetEvent(false);
            var webSocketClient = Substitute.For<IWebSocketClient>();
            webSocketClient.IsOpen.Returns(true);
            webSocketClient.When(x => x.Open()).DoNotCallBase();
            webSocketClient.When(x => x.Close()).DoNotCallBase();
            webSocketClient.When(x => x.Dispose()).DoNotCallBase();
            webSocketClient.WhenForAnyArgs(x => x.Send("")).Do(callInfo =>
            {
                var message = (string)callInfo.Args()[0];
                var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(message);
                if (webSocketMessage.Command == WebSocketCommand.GetXCApiList)
                {
                    var response = JsonConvert.DeserializeObject<WebSocketGetXcApiListResponse>(webSocketMessage.Json);
                    ExpectedXCApiList = response?.Apis != null ? new List<string>(response.Apis) : new List<string> {"Empty"};
                    requestSentEvent.Set();
                }
            });

            var xcApiManager = new WebSocketXCApiManager(webSocketClient);
            xcApiManager.GetXCApiList();
            var requestWasSent = requestSentEvent.WaitOne(TimeSpan.FromSeconds(5));

            Check.That(requestWasSent).IsTrue();
            Check.That(ExpectedXCApiList != null).IsTrue();
            Check.That(ExpectedXCApiList.Count == 1).IsTrue();
            Check.That(ExpectedXCApiList.Contains("Empty")).IsTrue();
        }
    }
}
