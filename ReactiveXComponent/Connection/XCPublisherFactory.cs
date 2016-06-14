using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Connection
{
    public class XCPublisherFactory : IXCPublisherFactory
    {
        private readonly XCConfiguration _xcConfiguration;

        public XCPublisherFactory(XCConfiguration configuration)
        {
            _xcConfiguration = configuration;
        }

        public IXCPublisher CreatePublisher()
        {
            return new XCPublisher(_xcConfiguration);
        }
    }
}
