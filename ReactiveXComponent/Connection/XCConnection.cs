using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public class XCConnection : IXCConnection
    {
        private bool _disposed;
        private readonly IXCSessionFactory _sessionFactory;

        private XCConnection(Stream xcApiStream)
        {
            _sessionFactory = new XCSessionFactory(xcApiStream);
        }

        public static IXCConnection CreateConnection(Stream xcApiStream)
        {
            //create a RabbitMQ or a webSocket Connection depending
            //on the info given by xcApiStream
            return new XCConnection(xcApiStream);
        }

        public IXCSession CreateSession()
        {
            return _sessionFactory?.CreateSession();
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_disposed)
                return;

            if (disposing)
            {
                _sessionFactory.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}