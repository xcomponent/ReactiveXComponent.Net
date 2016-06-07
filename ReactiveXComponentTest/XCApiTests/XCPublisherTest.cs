using System;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;
using ReactiveXComponent.XCApi;

namespace ReactiveXComponentTest.XCApiTests
{
    [TestFixture]
    public class XCPublisherTest
    {
        private string _engine;
        private string _component;
        private string _stateMachine;
        private int _eventCode;
        private string _messageType;
        private object _message;
        private Visibility _visibility;
        private IRabbitMqPublisher _publisher;
        private XCPublisher _xcPublisher;

        [SetUp]
        public void Setup()
        {
            _engine = String.Empty;
            _component = "ExchangeManager";
            _stateMachine = "ExchangeManager";
            _eventCode = 102;
            _messageType = String.Empty;
            _message = new object();
            _visibility = Visibility.Private;

            var parser = Substitute.For<DeploymentParser>("XCApiTests//PerseusApi.xcApi");
            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            rabbitMqConnection.GetConnection()?.IsOpen.Returns(true);
            var publisherFactory = Substitute.For<IRabbitMqPublisherFactory>();
            _publisher = Substitute.For<RabbitMqPublisher>(String.Empty, rabbitMqConnection);
            publisherFactory.Create(_component).Returns(_publisher);
            _xcPublisher = new XCPublisher(publisherFactory, parser);
            _xcPublisher.InitPrivateCommunication(null);
        }

        [TestCase()]
        public void SendEvent_GivenAppropriateArgs_ShouldCallPublisherSendWithAppropriateArgs_Test()
        {
            _xcPublisher.CreatePublisher(_component);
            _xcPublisher.SendEvent(_engine, _component, _stateMachine, _eventCode, _messageType, _message, _visibility);

            _publisher.ReceivedWithAnyArgs().Send(null, null, null);
        }

        [TearDown]
        public void TearDown()
        {
            _publisher.Close();
            _xcPublisher.Dispose();
        }
    }
}
