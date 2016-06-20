using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.UnitTests.RabbitMqUnitTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqConnectionTest : RabbitMqTestBase
    {
        [SetUp]
        protected override void Setup()
        {
            var busDetails = new BusDetails
            {
                Host = "127.0.0.1",
                Password = "guest" ,
                Port = 5672, 
                Username = "guest"
            };
            XCConfiguration.GetBusDetails().Returns(busDetails);
        }

        [Test]
        public void CreateSession_GivenXCConfiguration_ShouldReturnAnInstanceOfRabbitMqSession_Test()
        {
            using (var rabbitMqConnection = new RabbitMqConnection(XCConfiguration))
            {
                var rabbitMqSession = rabbitMqConnection.CreateSession();
                Check.That(rabbitMqSession).IsInstanceOf<RabbitMqSession>();
            }    
        }
    }
}
