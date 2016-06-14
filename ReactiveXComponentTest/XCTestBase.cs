using System;
using System.IO;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest
{
    public abstract class XCTestBase : IDisposable
    {
        private bool _disposed;
        protected Stream XCApiStream;
        private Tags _tags;
        protected Parser Parser;
        protected XCConfiguration XCConfiguration;

        [SetUp]
        public void PrivateSetup()
        {
            XCApiStream = new FileStream("TestApi.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            _tags = new Tags();
            Parser = new Parser(XCApiStream);
            Parser.Parse(_tags);
            XCConfiguration = new XCConfiguration(Parser);

            Setup();
        }

        protected virtual void Setup() { }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                XCApiStream.Dispose();
            }
            _disposed = true;
        }

        [TearDown]
        public void Dispose()
        {
            Dispose(true);
        }
    }
}