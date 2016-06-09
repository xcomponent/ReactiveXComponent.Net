﻿using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public class XCSession : IXCSession
    {
        private bool _disposed;
        private readonly Stream _xcApiStream;
        private readonly IXCPublisherFactory _publisherFactory; 

        public XCSession(Stream xcApiStream)
        {
            _xcApiStream = xcApiStream;
            _publisherFactory = new XCPublisherFactory(xcApiStream);
        }

        public IXCPublisher CreatePublisher(string component)
        {
            return _publisherFactory.CreatePublisher(component);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _xcApiStream?.Dispose();
                _publisherFactory?.Dispose();
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