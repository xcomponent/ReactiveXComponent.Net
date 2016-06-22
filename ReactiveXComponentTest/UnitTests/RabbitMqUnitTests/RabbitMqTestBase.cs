using System;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponentTest.UnitTests.RabbitMqUnitTests
{
    public abstract class RabbitMqTestBase : IDisposable
    {
        private bool _disposed;
        protected IConnection Connection;
        protected IXCConfiguration XCConfiguration;

        [SetUp]
        public void PrivateSetup()
        {
            Connection = Substitute.For<IConnection>();
            Connection.IsOpen.Returns(true);
            XCConfiguration = Substitute.For<IXCConfiguration>();
            
            Setup();
        }

        protected virtual void Setup() { }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Connection.Close();
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