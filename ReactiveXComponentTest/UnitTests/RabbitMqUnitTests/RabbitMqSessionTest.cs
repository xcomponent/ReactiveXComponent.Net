using NFluent;
using NUnit.Framework;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.UnitTests.RabbitMqUnitTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqSessionTest : RabbitMqTestBase
    {
        private string _component; 
        [SetUp]
        public void SetUp()
        {
            _component = "HelloWorld";
        }

        [Test]
        public void CreatePublisher_GivenAConfigAndAConnection_ShouldReturnAValidPublisher()
        {
            var session = new RabbitMqSession(XCConfiguration, Connection);
            var publisher = session?.CreatePublisher(_component);
            Check.That(publisher).IsInstanceOf<RabbitMqPublisher>();
        }
    }
}
