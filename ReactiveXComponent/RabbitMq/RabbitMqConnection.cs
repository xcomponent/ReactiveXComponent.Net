using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqConnection : IXCConnection
    {
        private readonly XCConfiguration _xcConfiguration;

        public RabbitMqConnection(XCConfiguration configuration)
        {
            _xcConfiguration = configuration;
        }

        public IXCSession CreateSession()
        {
            return new RabbitMqSession(_xcConfiguration);
        }
    }
}