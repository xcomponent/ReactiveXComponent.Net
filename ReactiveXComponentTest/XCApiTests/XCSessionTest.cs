using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;
using ReactiveXComponent.XCApi;

namespace ReactiveXComponentTest.XCApiTests
{
    [TestFixture]
    public class XCSessionTest
    {
        private string _component;
        private string _stateMachine;
        private XCSession _xcSession;
        [SetUp]
        public void Setup()
        {
            _component = "CheckOrder";
            _stateMachine = "CheckComponent";

            var publisherFactory = Substitute.For<IRabbitMqPublisherFactory>();
            var consumerFactory = Substitute.For<IRabbitMqConsumerFactory>();
            var parser = Substitute.For<DeploymentParser>("XCApiTests//PerseusApi.xcApi");
            _xcSession = new XCSession(publisherFactory, consumerFactory, parser);

        }

        [TestCase()]
        public void CreatePublisher_GivenAComponent_ShouldCreatePublisher_Test()
        {
            using (_xcSession)
            {
                _xcSession.CreatePublisher(_component);
                Check.That(_xcSession.XCPublisher).IsInstanceOf<XCPublisher>();
            }  
        }

        [TestCase()]
        public void CreateConsumer_GivenAComponent_ShouldCreateConsumer_Test()
        {
            using (_xcSession)
            {
                _xcSession.CreateConsumer(_component, _stateMachine);
                Check.That(_xcSession.XCConsumer).IsInstanceOf<XCConsumer>();
            }   
        }

        [TearDown]
        public void TearDown()
        {
            _xcSession.Dispose();
        }
    }
}
