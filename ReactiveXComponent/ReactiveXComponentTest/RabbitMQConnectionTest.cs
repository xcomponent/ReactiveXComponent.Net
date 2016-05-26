using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using global::RabbitMQ.Client;
using NSubstitute;
using ReactiveXComponent.RabbitMQ;


namespace ReactiveXComponent
{
    [TestFixture]
    public class RabbitMQConnectionTest
    {
        BusDetails busInfos = new BusDetails
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
            busInfos.Username = username;
            busInfos.Password = password;
            
            //Act
            var connect = new RabbitMqConnection(busInfos);
            var connection = connect.GetConnection();
            var isOpen = connection.IsOpen;

            //Assert
            Assert.IsTrue(isOpen);
        }
    }
}
