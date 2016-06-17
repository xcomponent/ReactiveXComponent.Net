using System;
using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.IntegrationTests.ConfiguartionTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class XCConfigutaionTest : IDisposable
    {
        private bool _disposed;
        private XCApiConfigParser _parser;
        private Stream _xcApiStream;
        private string _component;
        private string _stateMachine;
        private int _eventCode; 

        [SetUp]
        public void Setup()
        {
            _parser = new XCApiConfigParser();
            _xcApiStream = new FileStream("XCConfigIntegrationTests.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            _component = "HelloWorld";
            _stateMachine = "HelloWorldManager";
            _eventCode = 9;
        }

        [Test]
        public void GetBusDetails_GivenXCConfigurationInitilizedWithNull_ShouldThrowANullReferenceException_Test()
        {
            var xcConfig = new XCConfiguration(null);
            Check.ThatCode(() => xcConfig.GetBusDetails()).Throws<NullReferenceException>();
        }

        [Test]
        public void Init_GivenNullAndXCConfigurationInitilizedWithAParser_ShouldThrowFailedToInitException_Test()
        {
            var xcConfig = new XCConfiguration(_parser);
            Check.ThatCode(() => xcConfig.Init(null)).Throws<Exception>();
        }

        [Test]
        public void Init_GivenAStreamAndXCConfigurationInitilizedWithNull_ShouldThrowFailedToInitException_Test()
        {
            var xcConfig = new XCConfiguration(null);
            Check.ThatCode(() => xcConfig.Init(_xcApiStream)).Throws<Exception>();
        }

        [Test]
        public void GetPublisherTopic_GivenAStreamAndXCConfigurationInitilizedWithAParser_ShouldReturnANullPublisher_Test()
        {
            var xcConfig = new XCConfiguration(_parser);
            xcConfig.Init(_xcApiStream);
            var publisher = xcConfig.GetPublisherTopic(_component, _stateMachine, _eventCode);
            Check.That(publisher).IsNull();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _xcApiStream.Dispose();
                _parser = null;
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
