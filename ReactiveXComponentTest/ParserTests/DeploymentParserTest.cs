using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.ParserTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class DeploymentParserTest : XCTestBase
    {
        private string _component;
        private string _stateMachine;

        protected override void Setup()
        {
            _component = "HelloWorld";
            _stateMachine = "HelloWorldManager";
        }

        [Test]
        public void GetComponentCode_GivenComponent_ShouldReturnTheComponentIddentifier_Test()
        {
            var parser = new DeploymentParser(XCApiStream);
            const int expectedIdentifier = -69981087;
            var componentCode = parser.GetComponentCode(_component);

            Check.That(componentCode).IsEqualTo(expectedIdentifier);
        }

        [Test]
        public void GetStateMachineCode_GivenComponentAndStateMachine_ShouldReturnTheStateMachineIddentifier_Test()
        {
            var parser = new DeploymentParser(XCApiStream);
            const int expectedIdentifier = -829536631;
            var stateMachineCode = parser.GetStateMachineCode(_component, _stateMachine);

            Check.That(stateMachineCode).IsEqualTo(expectedIdentifier);
        }

        [Test]
        public void GetGetPublisherEventCode_GivenAMessageType_ShouldReturnThePublisherEventCode_Test()
        {
            var parser = new DeploymentParser(XCApiStream);
            const int expectedEventCode = 9;
            const string messageType = "XComponent.HelloWorld.UserObject.SayHello";
            var eventCode = parser.GetPublisherEventCode(messageType);

            Check.That(eventCode).IsEqualTo(expectedEventCode);
        }

        [Test]
        public void GetPublisherTopic_GiventComponentStateMachienAndEventCode_ShouldReturnAPublisherTopic_Test()
        {
            var parser = new DeploymentParser(XCApiStream);
            const string messageType = "XComponent.HelloWorld.UserObject.SayHello";
            var eventCode = parser.GetPublisherEventCode(messageType);
            const string expectedTopic = "input.1_0.HelloMicroservice.HelloWorld.HelloWorldManager";
            var topic = parser.GetPublisherTopic(_component, _stateMachine, eventCode);

            Check.That(topic).IsEqualTo(expectedTopic);
        }

        [Test]
        public void GetConsumerTopic_GiventComponentAndStateMachien_ShouldReturnAConsumerTopic_Test()
        {
            var parser = new DeploymentParser(XCApiStream);
            const string stateMachine = "HelloResponse";
            const string expectedTopic = "output.1_0.HelloMicroservice.HelloWorld.HelloResponse";
            var topic = parser.GetConsumerTopic(_component, stateMachine);

            Check.That(topic).IsEqualTo(expectedTopic);
        }
    }
}
