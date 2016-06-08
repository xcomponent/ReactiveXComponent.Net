using System;
using System.IO;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private bool _disposed;
        private readonly Stream _file;
        private readonly IXCSessionFactory _sessionFactory;

        public XComponentApi(Stream file)
        {
            _file = file;
            _sessionFactory = new XCSessionFactory();
        }

        public XCSession CreateSession()
        {
            return (_sessionFactory?.CreateSession(_file) as XCSession);
        }

        private void Close()
        {
            _file?.Close();
            _sessionFactory?.Close();
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
                return;
            Close();
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(_disposed);
            GC.SuppressFinalize(this);
        }

    }
}
