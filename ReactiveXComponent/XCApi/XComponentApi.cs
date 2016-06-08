using System;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public class XComponentApi : IXComponentApi
    {
        private bool _disposed = false;
        
        private readonly XCConnection _xcConnection;
        private readonly XCSessionFactory _xcSessionFactory;
        private XCSession _xcSession;

        public XComponentApi(IRabbitMqPublisherFactory publisherFactory, IRabbitMqConsumerFactory consumerFactory, IRabbitMqConnection connection, DeploymentParser parser)
        {
            _xcConnection = new XCConnection(connection);
            _xcSessionFactory = new XCSessionFactory(publisherFactory, consumerFactory, parser);
        }

        public void CreateSession()
        {
            _xcSession = _xcSessionFactory.CreateSession() as XCSession;
        }

        public void Close()
        {
            _xcConnection.Close();
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
                return;
            else
            {
                Close();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(_disposed);
            GC.SuppressFinalize(this);
        }

    }
}
