using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public class XCPublisher : IXCPublisher
    {
        private bool _disposed; 

        public XCPublisher(string component, Stream file)
        {
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                
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