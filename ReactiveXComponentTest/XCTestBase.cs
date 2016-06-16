using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest
{
    public abstract class XCTestBase : IDisposable
    {
        private bool _disposed;
        private Stream _xcApiStream;
        protected XCApiConfigParser Parser;
        protected XCConfiguration XCConfiguration;

        [SetUp]
        public void PrivateSetup()
        {
            _xcApiStream = new FileStream("TestApi.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            Parser = new XCApiConfigParser();
            XCConfiguration = new XCConfiguration(Parser);
            XCConfiguration.Init(_xcApiStream);
            Setup();
        }

        protected virtual void Setup() { }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _xcApiStream.Dispose();
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