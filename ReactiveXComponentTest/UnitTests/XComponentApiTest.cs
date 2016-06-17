using System;
using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.UnitTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class XComponentApiTest : IDisposable
    {
        private bool _disposed;
        private Stream _xcApiStream;

        [SetUp]
        public void Setup()
        {
            _xcApiStream = new FileStream("TestApi.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
        }

        [Test]
        public void CreateFromXCApi_GivenXCApiStreamFile_ShouldReturnNewInstanceOfXCApi()
        {
            var xcApi = XComponentApi.CreateFromXCApi(_xcApiStream);
            Check.That(xcApi).IsInstanceOf<XComponentApi>();
        }

        [Test]
        public void CreateSession_GivenXCApiStreamFile_ShouldReturnNewInstanceSession()
        {
            var xcApi = XComponentApi.CreateFromXCApi(_xcApiStream) as XComponentApi;
            var session = xcApi?.CreateSession();
            Check.That(session).IsInstanceOf<RabbitMqSession>();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _xcApiStream?.Dispose();
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
