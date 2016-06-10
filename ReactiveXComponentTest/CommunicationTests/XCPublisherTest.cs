using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
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

        [SetUp]
        protected override void Setup()
        {
            _component = "ExchangeManager";
            _stateMachine = "ExchangeManager";
            _message = new object();
            _visibility = Visibility.Private;
        }

        [Test]
        public void SendEvent_GivenAComponentAStateMachineAnObjectAVisibility_ShouldCreateAHeaderAndSendItToEngine_Test()
        {
            var parser = new DeploymentParser(XCApiStream);
            var publisher = new XCPublisher(parser);
            Check.ThatCode(() => publisher.SendEvent(_component, _stateMachine, _message, _visibility)).DoesNotThrow();
        }
    }
}
