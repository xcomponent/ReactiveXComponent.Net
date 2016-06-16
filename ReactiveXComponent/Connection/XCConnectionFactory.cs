using ReactiveXComponent.Configuration;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent.Connection
{
    public class XCConnectionFactory : AbstractXCConnectionFactory
    {
        private readonly XCConfiguration _xcConfiguration; 

        public XCConnectionFactory(XCConfiguration xcConfiguration)
        {
            _xcConfiguration = xcConfiguration;        
        }

        public override IXCConnection CreateConnection()
        {
            return new RabbitMqConnection(_xcConfiguration);
        }
    }
}
