using System;
using System.IO;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.IntegrationTests.RabbitMqIntegrationTests
{
    public abstract class RabbitMqIntegrationTestBase : IDisposable
    {
        private bool _disposed;
        private XCApiConfigParser _parser;
        private Stream _xcApiStream;
        protected XCConfiguration XCConfiguration;

        [SetUp]
        public void PrivateSetup()
        {
            _parser = new XCApiConfigParser();
            XCConfiguration = new XCConfiguration(_parser);
            _xcApiStream = new FileStream("TestApi.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
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