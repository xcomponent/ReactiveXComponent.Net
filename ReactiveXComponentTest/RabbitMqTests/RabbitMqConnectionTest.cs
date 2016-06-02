using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest
{
    [TestFixture]
    public class RabbitMqConnectionTest
    {
        BusDetails _busInfos = new BusDetails
        {
            Host = "localhost",
            Username = "",
            Password = "",
            Port = 5672
        };

        [TestCase("guest","guest")]
        public void CreateConnection_Sucess_test(string username, string password)
        {
            //Arrange
            _busInfos.Username = username;
            _busInfos.Password = password;
            
            //Act
            var rabbitMqconnection = Substitute.For<RabbitMqConnection>(_busInfos);
            var connection = rabbitMqconnection.GetConnection();
            var isOpen = connection.IsOpen;

            //Assert
            Assert.IsTrue(isOpen);
        }
    }
}
