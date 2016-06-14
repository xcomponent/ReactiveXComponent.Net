using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.ParserTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class ParserTest : XCTestBase
    {
        private string _component;
        private string _stateMachine;
        private Tags _tags;

        protected override void Setup()
        {
            _component = "HelloWorld";
            _stateMachine = "HelloWorldManager";
            _tags = new Tags();
        }

        [Test]
        public void GetComponentCode_GivenComponent_ShouldReturnTheComponentIddentifier_Test()
        {
            const int expectedIdentifier = -69981087;
            var componentCode = Parser.GetComponentCode(_component);

            Check.That(componentCode).IsEqualTo(expectedIdentifier);
        }

        [Test]
        public void GetStateMachineCode_GivenComponentAndStateMachine_ShouldReturnTheStateMachineIddentifier_Test()
        {
            const int expectedIdentifier = -829536631;
            var stateMachineCode = Parser.GetStateMachineCode(_component, _stateMachine);

            Check.That(stateMachineCode).IsEqualTo(expectedIdentifier);
        }

        [Test]
        public void GetGetPublisherEventCode_GivenAMessageType_ShouldReturnThePublisherEventCode_Test()
        {
            const int expectedEventCode = 9;
            const string messageType = "XComponent.HelloWorld.UserObject.SayHello";
            var eventCode = Parser.GetPublisherEventCode(messageType);

            Check.That(eventCode).IsEqualTo(expectedEventCode);
        }

        [Test]
        public void GetPublisherTopic_GiventComponentStateMachienAndEventCode_ShouldReturnAPublisherTopic_Test()
        {
            const string messageType = "XComponent.HelloWorld.UserObject.SayHello";
            var eventCode = Parser.GetPublisherEventCode(messageType);
            const string expectedTopic = "input.1_0.HelloMicroservice.HelloWorld.HelloWorldManager";
            var topic = Parser.GetPublisherTopic(_component, _stateMachine, eventCode);

            Check.That(topic).IsEqualTo(expectedTopic);
        }

        [Test]
        public void GetConsumerTopic_GiventComponentAndStateMachien_ShouldReturnAConsumerTopic_Test()
        {
            const string stateMachine = "HelloResponse";
            const string expectedTopic = "output.1_0.HelloMicroservice.HelloWorld.HelloResponse";
            var topic = Parser.GetConsumerTopic(_component, stateMachine);

            Check.That(topic).IsEqualTo(expectedTopic);
        }
    }
}
