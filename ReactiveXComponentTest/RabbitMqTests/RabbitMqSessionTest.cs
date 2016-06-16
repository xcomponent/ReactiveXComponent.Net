using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponentTest.ParserTests;

namespace ReactiveXComponentTest.CommunicationTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqSessionTest : XCTestBase
    {
        
        [Test]
        public void CreatePublisher_GivenNoArgs_ShouldReturnAValidPublisher()
        {
            var session = new RabbitMqSession(XCConfiguration as XCConfiguration);
            var publisher = session?.CreatePublisher();
            Check.That(publisher).IsInstanceOf<RabbitMqPublisher>();
        }
    }
}
