using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public class XCConnection : IXCConnection
    {
        private bool _disposed;
        private readonly IXCSessionFactory _sessionFactory;

        public XCConnection(Stream xcApiStream)
        {
            _sessionFactory = new XCSessionFactory(xcApiStream);
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