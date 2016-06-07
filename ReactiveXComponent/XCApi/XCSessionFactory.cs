using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public class XCSessionFactory : IXCSessionFactory
    {
        private readonly IRabbitMqPublisherFactory _publisherFactory;
        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly DeploymentParser _parser;

        public XCSessionFactory(IRabbitMqPublisherFactory publisherFactory, IRabbitMqConsumerFactory consumerFactory, DeploymentParser parser)
        {
            _publisherFactory = publisherFactory;
            _consumerFactory = consumerFactory;
            _parser = parser;
        }

        public IXCSession CreateSession()
        {
            return new XCSession(_publisherFactory, _consumerFactory, _parser);
        }
    }
}
