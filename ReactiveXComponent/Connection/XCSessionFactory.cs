using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Connection
{
    public class XCSessionFactory : IXCSessionFactory
    {
        private readonly XCConfiguration _xcConfiguration;

        public XCSessionFactory(XCConfiguration xcConfiguration)
        {
            _xcConfiguration = xcConfiguration;
        }

        public IXCSession CreateSession()
        {
            return new XCSession(_xcConfiguration);
        }
    }
}