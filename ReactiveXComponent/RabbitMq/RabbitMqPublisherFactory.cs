using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqPublisherFactory : IRabbitMqPublisherFactory
    {
        private readonly XCConfiguration _xcConfiguration;

        public RabbitMqPublisherFactory(XCConfiguration configuration)
        {
            _xcConfiguration = configuration;
        }

        public IXCPublisher CreatePublisher()
        {
            return new RabbitMqPublisher(_xcConfiguration);
        }
    }
}
