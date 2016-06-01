using NUnit.Framework;
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

        [TestCase("test", "test")]
        [TestCase("guest","guest")]
        public void CreateConnection_Sucess_test(string username, string password)
        {
            //Arrange
            _busInfos.Username = username;
            _busInfos.Password = password;
            
            //Act
            var connect = new RabbitMqConnection(_busInfos);
            var connection = connect.GetConnection();
            var isOpen = connection.IsOpen;

            //Assert
            Assert.IsTrue(isOpen);
        }
    }
}
