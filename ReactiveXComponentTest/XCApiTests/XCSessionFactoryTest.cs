using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;
using ReactiveXComponent.XCApi;

namespace ReactiveXComponentTest.XCApiTests
{
    [TestFixture]
    public class XCSessionFactoryTest
    {
        private XCSessionFactory _xcSessionFactory;

        [SetUp]
        public void Setup()
        {
            var publisherFactory = Substitute.For<IRabbitMqPublisherFactory>();
            var consumerFactory = Substitute.For<IRabbitMqConsumerFactory>();
            var parser = Substitute.For<DeploymentParser>("XCApiTests//PerseusApi.xcApi");

            _xcSessionFactory = new XCSessionFactory(publisherFactory, consumerFactory, parser);
        }

        [TestCase()]
        public void CreateSession_GivenNoArgs_ShouldReturnANewInstanceOfSession_Test()
        {
            var session = _xcSessionFactory.CreateSession();
            Check.That(session).IsInstanceOf<XCSession>();
        }
    }
}
