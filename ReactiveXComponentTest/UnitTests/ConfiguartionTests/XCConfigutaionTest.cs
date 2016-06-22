using System;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.UnitTests.ConfiguartionTests
{
    [TestFixture]
    public class XCConfigutaionTest
    {
        private IXCApiConfigParser _parser;

        [SetUp]
        public void Setup()
        {
            _parser = Substitute.For<IXCApiConfigParser>();
        }

        [Test]
        public void GetBusDetails_GivenXCConfigurationInitilizedWithNull_ShouldThrowANullReferenceException_Test()
        {
            var xcConfig = new XCConfiguration(null);
            Check.ThatCode(() => xcConfig.GetBusDetails()).Throws<NullReferenceException>();
        }

        [Test]
        public void Init_GivenAStreamAndXCConfigurationInitilizedWithNull_ShouldThrowFailedToInitException_Test()
        {
            var xcConfig = new XCConfiguration(null);
            Check.ThatCode(() => xcConfig.Init(null)).Throws<NullReferenceException>();
        }

        [Test]
        public void GetPublisherTopic_GivenAStreamAndXCConfigurationInitilizedWithAParser_ShouldReturnANullPublisher_Test()
        {
            var xcConfig = new XCConfiguration(_parser);
            xcConfig.Init(null);
            var publisher = xcConfig.GetPublisherTopic("", "", 0);
            Check.That(publisher).IsEqualTo(string.Empty);
        }
    }
}
