using System;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;
using ReactiveXComponent.XCApi;

namespace ReactiveXComponentTest.XCApiTests
{
    [TestFixture]
    public class XCConsumerTest
    {
        private string _component;
        private string _stateMachine;
        private string _topic;
        private IConsumer _consumer;
        private XCConsumer _xcConsumer;
        private Action<MessageEventArgs> _callback0;
        private Action<MessageEventArgs> _callback1;
        private Action<MessageEventArgs> _callback2;

        [SetUp]
        public void Setup()
        {
            var consumerFactory = Substitute.For<IRabbitMqConsumerFactory>();
            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            rabbitMqConnection.GetConnection().IsOpen.Returns(true);
            var parser = Substitute.For<DeploymentParser>("XCApiTests//PerseusApi.xcApi");

            _component = "CheckOrder";
            _stateMachine = "CheckComponent";
            _topic = "output.0_2_0_10001.engine1.CheckOrder.CheckComponent";
            _consumer = Substitute.For<IConsumer>();
            consumerFactory.Create(_component, _topic).Returns(_consumer);

            _callback0 = new Action<MessageEventArgs>(x => { });
            _callback1 = new Action<MessageEventArgs>(x => { });
            _callback2 = new Action<MessageEventArgs>(x => { });

            _xcConsumer = new XCConsumer(consumerFactory, parser);
        }

        [TestCase()]
        public void AddCallBack_GivenACallbackAComponentAndAStateMachine_ShouldCallConsumerMethodStart_Test()
        {
            using (_xcConsumer)
            {
                _xcConsumer.InitPrivateCommunication(null);
                _xcConsumer.CreateConsummer(_component, _stateMachine);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback0);
            }

            _consumer.ReceivedWithAnyArgs().Start();
        }

        public void AddCallBack_GivenACallbackAComponentAndAStateMachine_ShouldCallConsumerMethodStart_PrivateTopicCase_Test()
        {
            using (_xcConsumer)
            {
                _xcConsumer.InitPrivateCommunication("PrivateCommunicationIdentifier");
                _xcConsumer.CreateConsummer(_component, _stateMachine);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback0);
            }

            _consumer.ReceivedWithAnyArgs().Start();
        }

        [TestCase()]
        public void RemoveCallBack_GivenSeveralCallbacks_ShouldRemoveAll_Test()
        {
            const int callbacksToStop = 3;

            using (_xcConsumer)
            {
                _xcConsumer.InitPrivateCommunication(null);
                _xcConsumer.CreateConsummer(_component, _stateMachine);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback0);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback1);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback2);
            }

            _consumer.Received(callbacksToStop).Stop();
        }

        [TestCase()]
        public void RemoveCallBack_GivenSeveralCallbacks_ShouldRemoveAll_PrivateTopicCase_Test()
        {
            const int callbacksToStop = 3;

            using (_xcConsumer)
            {
                _xcConsumer.InitPrivateCommunication("PrivateCommunicationIdentifier");
                _xcConsumer.CreateConsummer(_component, _stateMachine);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback0);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback1);
                _xcConsumer.AddCallback(_component, _stateMachine, _callback2);
            }

            _consumer.Received(callbacksToStop).Stop();
        }

        [TestCase()]
        public void RemoveCallBack_GivenSeveralCallbacks_ShouldRemoveTwo_Test()
        {
            const int callbacksToStop = 2;

            _xcConsumer.InitPrivateCommunication(null);
            _xcConsumer.CreateConsummer(_component, _stateMachine);
            _xcConsumer.AddCallback(_component, _stateMachine, _callback0);
            _xcConsumer.AddCallback(_component, _stateMachine, _callback1);
            _xcConsumer.AddCallback(_component, _stateMachine, _callback2);

            _xcConsumer.RemoveCallback(_callback0);
            _xcConsumer.RemoveCallback(_callback2);

            _consumer.Received(callbacksToStop).Stop();
        }

        [TearDown]
        public void TearDown()
        {
            _consumer.Stop();
            _xcConsumer.Dispose();
        }
    }
}
