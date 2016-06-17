using System;
using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.IntegrationTests.RabbitMqIntegrationTests
{
    [TestFixture]
    [Category("Intégration tests")]
    public class RabbitMqConnectionTest
    {
        private bool _disposed;
        private XCConfiguration _xcConfiguration;

        [SetUp]
        public void Setup()
        {
            var parser = new XCApiConfigParser();
            _xcConfiguration = new XCConfiguration(parser);
            var xcApiStream = new FileStream("RabbitMqConnectionIntegrationTests.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            _xcConfiguration.Init(xcApiStream);
        }

        [Test]
        public void RabbitMqConnection_GivenAStreamWithoutAUserIdentifier_ShouldConnectToRabbitMqUsingDefaultUser_Test()
        {
            Check.ThatCode(() => new RabbitMqConnection(_xcConfiguration)).DoesNotThrow();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _xcConfiguration = null;
            }
            _disposed = true;

        }

        [TearDown]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
