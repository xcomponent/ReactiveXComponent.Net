using System;
using System.IO;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private bool _disposed;
        private readonly IXCConnection _xcConnection;       

        private XComponentApi(Stream xcApiStream)
        {
            _xcConnection = new XCConnection(xcApiStream);
        }

        public static IXComponentApi CreateFromXCApi(Stream xcApiStream)
        {
            return new XComponentApi(xcApiStream);
        }

        public IXCSession CreateSession()
        {
            return _xcConnection.CreateSession();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _xcConnection?.Dispose();
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
