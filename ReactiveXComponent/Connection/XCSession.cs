﻿using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public class XCSession : IXCSession
    {
        private bool _disposed;
        private readonly Stream _xcApiStream; 

        public XCSession(Stream xcApiStream)
        {
            _xcApiStream = xcApiStream;
        }

        public IXCPublisher CreatePublisher(string component)
        {
            return new XCPublisher(component, _xcApiStream);
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