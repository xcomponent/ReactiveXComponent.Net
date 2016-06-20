using NFluent;
using NUnit.Framework;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.UnitTests.RabbitMqUnitTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqSessionTest : RabbitMqTestBase
    {
        [Test]
        public void CreatePublisher_GivenAConfigAndAConnection_ShouldReturnAValidPublisher()
        {
            var session = new RabbitMqSession(XCConfiguration, Connection);
            var publisher = session.CreatePublisher(string.Empty);
            Check.That(publisher).IsInstanceOf<RabbitMqPublisher>();
        }
    }
}
