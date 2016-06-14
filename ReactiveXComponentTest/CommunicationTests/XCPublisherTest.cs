using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.CommunicationTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class XCPublisherTest : XCTestBase 
    {
        private string _component;
        private string _stateMachine;
        private object _message;
        private Visibility _visibility;
        private Tags _tags;

        [SetUp]
        protected override void Setup()
        {
            _component = "HelloWorld";
            _stateMachine = "HelloWorldManager";
            _message = new object();
            _visibility = Visibility.Private;
            _tags = new Tags();
        }

        [Test]
        public void SendEvent_GivenAComponentAStateMachineAnObjectAVisibility_ShouldCreateAHeaderAndSendItToEngine_Test()
        {
            var publisher = new XCPublisher(XCConfiguration);
            Check.ThatCode(() => publisher.SendEvent(_component, _stateMachine, _message, _visibility)).DoesNotThrow();
        }
    }
}
