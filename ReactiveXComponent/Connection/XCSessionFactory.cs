using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public class XCSessionFactory : IXCSessionFactory
    {
        private bool _disposed;
        private readonly Stream _xcApiStream;

        public XCSessionFactory(Stream xcApiStream)
        {
            _xcApiStream = xcApiStream;
        }

        public IXCSession CreateSession()
        {
            return new XCSession(_xcApiStream);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _xcApiStream?.Dispose();
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