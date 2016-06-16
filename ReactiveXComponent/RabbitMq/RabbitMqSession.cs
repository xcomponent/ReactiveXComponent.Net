using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSession : IXCSession
    {
        private readonly IRabbitMqPublisherFactory _publisherFactory;

        public RabbitMqSession(XCConfiguration xcConfiguration)
        {
            _publisherFactory = new RabbitMqPublisherFactory(xcConfiguration);
        }

        public IXCPublisher CreatePublisher()
        {
            return _publisherFactory.CreatePublisher();
        }
    }
}