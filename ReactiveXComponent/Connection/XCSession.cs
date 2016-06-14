using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Connection
{
    public class XCSession : IXCSession
    {
        private readonly IXCPublisherFactory _publisherFactory;

        public XCSession(XCConfiguration xcConfiguration)
        {
            _publisherFactory = new XCPublisherFactory(xcConfiguration);
        }

        public IXCPublisher CreatePublisher()
        {
            return _publisherFactory.CreatePublisher();
        }
    }
}