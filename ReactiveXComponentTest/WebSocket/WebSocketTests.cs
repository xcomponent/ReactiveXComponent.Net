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
            var expectedAnswer =
                "\r\n<deployment environment=\"Dev\" xcProjectName=\"helloworld\" deploymentTargetCode=\"1656928013\" deploymentTargetName=\"helloworldApi\" version=\"1.0\" "
                + "frameworkType=\"Framework45\" xmlns=\"http://xcomponent.com/DeploymentConfig.xsd\">\r\n  <threading />\r\n  <serialization>Json</serialization>\r\n  "
                + "<communication>\r\n    <bus name=\"rabbitmq\" host=\"127.0.0.1\" port=\"5672\" user=\"guest\" password=\"guest\" type=\"RABBIT_MQ\" />\r\n  "
                + "</communication>\r\n  <clientAPICommunication>\r\n    <publish componentCode=\"-69981087\" stateMachineCode=\"-829536631\" eventType=\"UPDATE\" "
                + "topicType=\"output\" communicationType=\"BUS\" stateCode=\"0\" eventCode=\"9\" event=\"XComponent.HelloWorld.UserObject.SayHello\" communication=\""
                + "rabbitmq\">\r\n      <topic type=\"STATIC\">input.1_0.microservice1.HelloWorld.HelloWorldManager</topic>\r\n    </publish>\r\n    <subscribe "
                + "componentCode=\"-69981087\" eventType=\"ERROR\" topicType=\"input\" communicationType=\"BUS\" communication=\"rabbitmq\">\r\n      <topic type=\"STATIC\">" 
                + "error.1_0.microservice1.HelloWorld</topic>\r\n    </subscribe>\r\n    <subscribe componentCode=\"-69981087\" stateMachineCode=\"1837059171\" eventType=\""
                + "UPDATE\" topicType=\"input\" communicationType=\"BUS\" stateCode=\"0\" event=\"XComponent.HelloWorld.UserObject.HelloResponse\" communication=\"rabbitmq\" "
                + "communicationThreadingType=\"INHERITFROMPARENT\">\r\n      <topic type=\"STATIC\">output.1_0.microservice1.HelloWorld.HelloResponse</topic>\r\n    "
                + "</subscribe>\r\n    <snapshot componentCode=\"-69981087\">\r\n      <topic type=\"STATIC\">snapshot.1_0.microservice1.HelloWorld</topic>\r\n    "
                + "</snapshot>\r\n  </clientAPICommunication>\r\n  <codesConverter>\r\n    <components>\r\n      <component name=\"HelloWorld\" id=\"-69981087\">\r\n        "
                + "<events>\r\n          <event name=\"XComponent.Common.Event.ApiProxy.ApiInitError\" id=\"0\" />\r\n          "
                + "<event name=\"XComponent.Common.Event.ApiProxy.ApiInitSuccessful\" id=\"1\" />\r\n          <event name=\"XComponent.Common.Event.ApiProxy.CancelApiInit\" " 
                + "id=\"2\" />\r\n          <event name=\"XComponent.Common.Event.ApiProxy.InstanceUpdatedSubscription\" id=\"3\" />\r\n          "
                + "<event name=\"XComponent.Common.Event.ApiProxy.InstanceUpdatedUnsubscription\" id=\"4\" />\r\n          "
                + "<event name=\"XComponent.Common.Event.ApiProxy.SnapshotOptions\" id=\"5\" />\r\n          <event name=\"XComponent.Common.Event.DefaultEvent\" id=\"6\" />\r\n"
                + "          <event name=\"XComponent.Common.Event.ExceptionEvent\" id=\"7\" />\r\n          <event name=\"XComponent.HelloWorld.UserObject.HelloResponse\" id=\"8\" />\r\n"
                + "          <event name=\"XComponent.HelloWorld.UserObject.SayHello\" id=\"9\" />\r\n        </events>\r\n        <stateMachines>\r\n          "
                + "<stateMachine name=\"HelloWorldManager\" id=\"-829536631\">\r\n            <states>\r\n              <State name=\"EntryPoint\" id=\"0\" />\r\n            "
                + "</states>\r\n          </stateMachine>\r\n          <stateMachine name=\"HelloResponse\" id=\"1837059171\">\r\n            <states>\r\n              "
                + "<State name=\"Done\" id=\"0\" />\r\n            </states>\r\n          </stateMachine>\r\n        </stateMachines>\r\n      </component>\r\n    "
                + "</components>\r\n  </codesConverter>\r\n</deployment>";
            var compressedXCApi =
                "\"H4sIAAAAAAAEAK2WbW / aMBDH31fqd4j8fnmA8jQBEgWqMonCQtD2rgqJAW / BzmyHwT79LokTApRCoeJNfLZ / 97 / z + cz9XdPHYcC2K0ylhumacEbj7xbq4TXSNt6Ys1 / Yky / uCrfQEgcB"
                + "+ 8t44CNtt89x + QLLLvNhhVWtVBulummVj1ccMjohQdoac0EYhZ26ibQ5hzUw + dvZhrD2KRs + VEDLKqAC9ksZfjWMjcdWIaMA1 + HL6OW + uozOyULfCB + 17 + 80rSmXHLs + oQvNSA0Cc "
                + "+ IG5J8rwXH7m2C0aezbkmWAXUWUeDsTGGeR0GgSB3dnMyJXf5C2ZALyZZVqugk / C2kh42CoVGslpEVAbqFFhIWECVcICMfPDTIJ0 + 48Pg6c1 + F3lEk0jp03vYBAeJ3xoPuWsDCaBUQstTwt6Xl8qT"
                + "Yadcus15AmpCvx0PWWhGI1WS81KuVqtQya8To + p0TOdNzrOH0Qx0LipSYWyTACvXuy0qnH6USxU6ipWOmooUYt9LObH9hzXAI / 4hLQp5Ce0SyuMH3ibpOJAy + FTKtY4zONpansTZyOM + iiNqEgUb"
                + "deTX1FPM4AvCYetoredp9Dl7oLzJtGQsqSaKgsZmMRzYTHyQy / k9ZC4vq2PbL38pZoOp226wLFnDP + bqCHYeVxfCSw43qx6uWaWWlYtbP1cibuN8rlggJJrDYWsEjg08k78Jrd / tT94OW5bw + cJ3s"
                + "0HHfs / otzJtdp4V9QVZmu86mnbiiWTJ7O / PuKsv0fKwC1Kesup1sJtD0fC2ii0Jgl5hkhFyt26nKb6og790gj / psRwa7ktEXBktkUpVAGsTpG9X48q8NrAW / RZht / DCiR / fgWpI7MrG / eBJx"
                + "EnoeFmEdBSrVuo3Zd6uFAsVNi6TbigMK9Aeg09OH6 + JO0ssL46FJ++VP5UyqOPDzc5mGiynCUIEXKrFzF7OG5GwUyGaSc6lWc / sbDiZoCqXYp6aI + FRPrtxF3T2MMa + zDmsbRlWoWe / fBXStOHV1c"
                + "9Syq + 7v7e7CHyCDiwAr2SWxX1D6VfDtmJMvq0S1NGtMxSBmVxMvE72e78E5dI7wHR / FJkvcnCr3TyJtn3qMPWmxsOOjETWP3l7r9HzBAksy7CwAA\"";
            var requestId = Guid.NewGuid().ToString();
            var expectedRequestProperty = string.Empty;
            var requestSentEvent = new AutoResetEvent(false);
            var webSocketClient = Substitute.For<IWebSocketClient>();
            webSocketClient.IsOpen.Returns(true);
            webSocketClient.When(x => x.Open()).DoNotCallBase();
            webSocketClient.When(x => x.Close()).DoNotCallBase();
            webSocketClient.When(x => x.Dispose()).DoNotCallBase();
            webSocketClient.WhenForAnyArgs(x => x.Send("")).Do(callInfo =>
            {
                var message = (string) callInfo.Args()[0];
                var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(message);
                if (webSocketMessage.Command == WebSocketCommand.GetXCApi)
                {
                    var xcApiProperties = JsonConvert.DeserializeObject<WebSocketXCApiRequest>(webSocketMessage.Json);
                    if(xcApiProperties.RequestId == requestId)
                    {
                        expectedRequestProperty = xcApiProperties.Name;
                        requestSentEvent.Set();
                    }

                    var data = "getXcApi {\"RequestId\": \""+requestId+"\",\"ApiFound\":true,\"ApiName\":\"HelloWorld.api\",\"Content\":" + compressedXCApi +"}";
                    var rawData = Encoding.UTF8.GetBytes(data);
                    var messageEventArgs = new WebSocketMessageEventArgs(data, rawData);
                    webSocketClient.MessageReceived += Raise.EventWith(messageEventArgs);
                }
            });

            var xcApiManager = new WebSocketXCApiManager(webSocketClient);
            var answer = xcApiManager.GetXCApi("HelloWorld.api", requestId);
            var requestWasSent = requestSentEvent.WaitOne(TimeSpan.FromSeconds(5));

            Check.That(requestWasSent).IsTrue();
            Check.That(expectedRequestProperty.Equals("HelloWorld.api")).IsTrue();
            Check.That(answer.Equals(expectedAnswer));
        }

        [Test]
        public void GetXCApiListTest()
        {
            var expectedAnswer = new List<string> {"helloWorld.api"};
            var requestId = Guid.NewGuid().ToString();
            var requestSentEvent = new AutoResetEvent(false);
            var webSocketClient = Substitute.For<IWebSocketClient>();
            webSocketClient.IsOpen.Returns(true);
            webSocketClient.When(x => x.Open()).DoNotCallBase();
            webSocketClient.When(x => x.Close()).DoNotCallBase();
            webSocketClient.When(x => x.Dispose()).DoNotCallBase();
            webSocketClient.WhenForAnyArgs(x => x.Send("")).Do(callInfo =>
            {
                var message = (string) callInfo.Args()[0];
                var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(message);
                if (webSocketMessage.Command == WebSocketCommand.GetXCApiList)
                {
                    requestSentEvent.Set();

                    var data = "getXcApiList {\"RequestId\": \""+requestId+"\",\"Apis\":[\"helloWorld.api\"]}";
                    var rawData = Encoding.UTF8.GetBytes(data);
                    var messageEventArgs = new WebSocketMessageEventArgs(data, rawData);
                    webSocketClient.MessageReceived += Raise.EventWith(messageEventArgs);
                }
            });

            var xcApiManager = new WebSocketXCApiManager(webSocketClient);
            var answer = xcApiManager.GetXCApiList(requestId);
            var requestWasSent = requestSentEvent.WaitOne(TimeSpan.FromSeconds(5));

            Check.That(requestWasSent).IsTrue();
            Check.That(answer.Count == expectedAnswer.Count);
            Check.That(answer.FirstOrDefault().Equals(expectedAnswer.FirstOrDefault()));
        }
    }
}
