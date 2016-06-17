using System;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Connection;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.IntegrationTests.RabbitMqIntegrationTests
{
    [TestFixture]
    [Category("Integration Tests")]
    public class RabbitMqPublisherTest : RabbitMqIntegrationTestBase
    {
        private string _component;
        //private string _stateMachine;
        //private object _message;
        private Visibility _visibility;
        private IXCSession _session;

        [SetUp]
        protected override void Setup()
        {
            _component = "HelloWorld";
            //_stateMachine = "HelloWorldManager";
            //_message = new SayHello();
            _visibility = Visibility.Private;
            var connection = new RabbitMqConnection(XCConfiguration);
            _session = connection.CreateSession();
        }

        //[Test]
        //public void SendEvent_GivenAComponentAStateMachineAnObjectAVisibility_ShouldInitHeaderAndThrowNoException_Test()
        //{
        //    var publisher = _session.CreatePublisher(_component);
        //    Check.ThatCode(() => publisher.SendEvent(_component, _stateMachine, _message, _visibility)).DoesNotThrow();
        //}

        [Test]
        public void SendEvent_GivenAComponentAStateMachineAnObjectAVisibility_ShouldInitHeaderAndThrowFailedToPublishException_Test()
        {
            var publisher = _session.CreatePublisher(_component);
            Check.ThatCode(() => publisher.SendEvent("","", new object(), _visibility)).Throws<Exception>();
        }

        [TearDown]
        public void TearDown()
        {
            //_message = null;
            _session = null;
        }
    }
}
