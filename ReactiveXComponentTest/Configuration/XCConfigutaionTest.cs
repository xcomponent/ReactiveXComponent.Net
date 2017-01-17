﻿using System;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.Configuration
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
        public void GetPublisherTopic_GivenAStreamAndXCConfigurationInitilizedWithAParser_ShouldReturnANullPublisher_Test()
        {
            var xcConfig = new XCConfiguration(_parser);
            xcConfig.Init(null);
            var publisher = xcConfig.GetPublisherTopic("", "");
            Check.That(publisher).IsEqualTo(string.Empty);
        }
    }
}