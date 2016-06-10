using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public class XCSessionFactory : IXCSessionFactory
    {
        private readonly Stream _xcApiStream;

        public XCSessionFactory(Stream xcApiStream)
        {
            _xcApiStream = xcApiStream;
        }

        public IXCSession CreateSession()
        {
            return new XCSession(_xcApiStream);
        }
    }
}