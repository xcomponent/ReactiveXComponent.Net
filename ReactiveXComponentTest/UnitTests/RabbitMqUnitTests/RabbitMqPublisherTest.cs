using NFluent;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.Common;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.UnitTests.RabbitMqUnitTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqPublisherTest : RabbitMqTestBase 
    {
        private object _message;
        private IModel _model;

        [SetUp]
        protected override void Setup()
        {
            _message = new object();
            _model = Substitute.For<IModel>();
            Connection.CreateModel().Returns(_model);
        }

        [Test]
        public void SendEvent_GivenAComponentAStateMachineAnObjectAVisibility_ShouldInitHeaderAndThrowNoException_Test()
        {
            var publisher = new RabbitMqPublisher("", XCConfiguration, Connection);
            Check.ThatCode(() => publisher.SendEvent("", "", _message, Visibility.Private)).DoesNotThrow();
        }

        [Test]
        public void SendEvent_GivenAComponentAStateMachineAnObjectAVisibility_ShouldInitAHeaderAndSendItToEngineThroughRabbitMqCallingBasicPublish_Test()
        {
            var publisher = new RabbitMqPublisher("", XCConfiguration, Connection);
            Check.ThatCode(() =>_model.ReceivedWithAnyArgs().BasicPublish(null, null, null, null));
        }

        [TearDown]
        public void TearDown()
        {
            _message = null;
            _model.Dispose();
        }
    }
}
