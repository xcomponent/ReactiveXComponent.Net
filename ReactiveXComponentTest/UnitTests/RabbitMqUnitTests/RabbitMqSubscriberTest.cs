using System;
using System.Collections.Generic;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest.UnitTests.RabbitMqUnitTests
{
    [TestFixture]
    public class RabbitMqSubscriberTest: RabbitMqTestBase
    {
        [SetUp]
        protected void SetUp()
        {
            var channel = Substitute.For<IModel>();
            Connection.CreateModel().Returns(channel);
            channel.QueueDeclare().Returns(new QueueDeclareOk(string.Empty,0,0));
        }

        [Test]
        public void AddCallback_GiventAComponentAStateMachineAndNoCallback_ShouldThrowNoException_Test()
        {
            using (var subscriber = new RabbitMqSubscriber(XCConfiguration,Connection))
            {
                var component = string.Empty;
                var stateMachine = string.Empty;
                Check.ThatCode(() =>subscriber?.AddCallback(component, stateMachine, null)).DoesNotThrow();
            }
        }

        [TestCase("initPrivateCommunication")]
        [TestCase(null)]
        public void AddCallback_GiventAComponentAStateMachineAndACallBack_ShouldThrowNoExceptionAndCreateSubscription_Test(string privateCommunicationIdentifier)
        {
            using (var subscriber = new RabbitMqSubscriber(XCConfiguration, Connection, privateCommunicationIdentifier))
            {
                Action<MessageEventArgs> callback = new Action<MessageEventArgs>(args => { });
                var component = string.Empty;
                var stateMachine = string.Empty;
                Check.ThatCode(() => subscriber?.AddCallback(component, stateMachine, callback)).DoesNotThrow();
            }
        }

        [Test]
        public void RemoveCallback_GiventAComponentAStateMachineAndACallback_ShouldThrowNoExceptionAndUnSubscribetheCallback_Test()
        {
            using (var subscriber = new RabbitMqSubscriber(XCConfiguration, Connection))
            {
                Action<MessageEventArgs> callback = new Action<MessageEventArgs>(args => { });
                var component = string.Empty;
                var stateMachine = string.Empty;
                subscriber?.AddCallback(component, stateMachine, callback);
                Check.ThatCode(() => subscriber?.RemoveCallback(component, stateMachine, callback)).DoesNotThrow();
            }
        }

        [Test]
        public void RemoveCallback_GiventAComponentAStateMachineAndNoCallback_ShouldNotFindCallbackAndThrowException_Test()
        {
            using (var subscriber = new RabbitMqSubscriber(XCConfiguration, Connection))
            {
                Action<MessageEventArgs> callback = new Action<MessageEventArgs>(args => { });
                var component = string.Empty;
                var stateMachine = string.Empty;
                subscriber?.AddCallback(component, stateMachine, null);
                Check.ThatCode(() => subscriber?.RemoveCallback(component, stateMachine, callback)).Throws<KeyNotFoundException>();
            }
        }

        [Test]
        public void RemoveCallback_GiventAComponentAStateMachineAndNoCallback_ShouldNotFindCallbackAndThrowNoException_Test()
        {
            using (var subscriber = new RabbitMqSubscriber(XCConfiguration, Connection))
            {
                Action<MessageEventArgs> callback = new Action<MessageEventArgs>(args => { });
                var component = string.Empty;
                var stateMachine = string.Empty;
                subscriber?.AddCallback(component, stateMachine, callback);
                subscriber?.RemoveCallback(component, stateMachine, null);
                Check.ThatCode(() => subscriber?.RemoveCallback(component, stateMachine, null)).DoesNotThrow();
            }
        }
    }
}
