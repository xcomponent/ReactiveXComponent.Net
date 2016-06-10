using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Connection;
using ReactiveXComponentTest.ParserTests;

namespace ReactiveXComponentTest.CommunicationTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class XCSessionTest : XCTestBase
    {
        [Test]
        public void CreatePublicher_GivenNoArgs_ShouldReturnAValidPublisher()
        {
            var session = new XCSession(XCApiStream);
            var publisher = session?.CreatePublisher();
            Check.That(publisher).IsInstanceOf<XCPublisher>();
        }

        [Test]
        public void
            InitPrivateCommunicationIdentifier_GivenAnIdentifier_ShouldInitPublisherPrivateIdentificationIdentifier()
        {
            var session = new XCSession(XCApiStream);
            const string privateCommunicationId = "id";
            session?.InitPrivateCommunicationIdentifier(privateCommunicationId);
            Check.That(XCPublisher.PrivateCommunicationIdentifier).IsEqualTo(privateCommunicationId);
        }
    }
}
