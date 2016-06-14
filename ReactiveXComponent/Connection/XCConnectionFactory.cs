using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Connection
{
    public class XCConnectionFactory : IXCConnectionFactory
    {
        private readonly XCConfiguration _xcConfiguration; 

        public XCConnectionFactory(XCConfiguration xcConfiguration)
        {
            _xcConfiguration = xcConfiguration;        
        }

        public IXCConnection CreateConnection()
        {
            switch (_xcConfiguration.GetConnectionType())
            {
                case "bus":
                    return new XCConnection(_xcConfiguration);
                case "websocket":
                    return new XCConnection(_xcConfiguration);
                default:
                    return null;
            }
        }
    }
}
