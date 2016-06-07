using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest
{
    [TestFixture]
    [Category("Integration Tests")]
    public class RabbitMqConnectionTest
    {
    //    private BusDetails _busInfos;
    //    [SetUp]
    //    public void Setup()
    //    {
    //        _busInfos = new BusDetails
    //        {
    //            Host = "localhost",
    //            Username = "",
    //            Password = "",
    //            Port = 5672
    //        };
    //    }
    //    
    //
    //    [TestCase("guest","guest")]
    //    public void CreateConnection_GivenBusDetails_ShouldOpenConnexion_test(string username, string password)
    //    {
    //        //Arrange
    //        _busInfos.Username = username;
    //        _busInfos.Password = password;
    //        bool isOpen;
    //        
    //        //Act
    //        using (var rabbitMqconnection = new RabbitMqConnection(_busInfos))
    //        {
    //            var connection = rabbitMqconnection.GetConnection();
    //            isOpen = connection.IsOpen;
    //        }
    //
    //        //Assert
    //        Assert.IsTrue(isOpen);
    //    }
    }
}
