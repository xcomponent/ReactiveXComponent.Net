using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;
using ReactiveXComponentTest.ParserTests;

namespace ReactiveXComponentTest.CommunicationTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class XCSessionTest : XCTestBase
    {
        
        [Test]
        public void CreatePublisher_GivenNoArgs_ShouldReturnAValidPublisher()
        {
            var session = new XCSession(XCConfiguration);
            var publisher = session?.CreatePublisher();
            Check.That(publisher).IsInstanceOf<XCPublisher>();
        }
    }
}
