using System.IO;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent.Connection
{
    public class XCSession : IXCSession
    {
        private readonly IXCPublisherFactory _publisherFactory;

        public XCSession(Stream xcApiStream)
        {
            var parser = new DeploymentParser(xcApiStream);
            _publisherFactory = new XCPublisherFactory(parser);
        }

        public void InitPrivateCommunicationIdentifier(string privateCommunicationIdentifier)
        {
            XCPublisher.PrivateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public IXCPublisher CreatePublisher()
        {
            return _publisherFactory.CreatePublisher();
        }
    }
}