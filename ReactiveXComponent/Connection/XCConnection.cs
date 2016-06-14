using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Connection
{
    public class XCConnection : IXCConnection
    {
        private readonly IXCSessionFactory _sessionFactory;

        public XCConnection(XCConfiguration configuration)
        {
            _sessionFactory = new XCSessionFactory(configuration);
        }

        public IXCSession CreateSession()
        {
            return _sessionFactory?.CreateSession();
        }

        public void Close()
        {
        }
    }
}