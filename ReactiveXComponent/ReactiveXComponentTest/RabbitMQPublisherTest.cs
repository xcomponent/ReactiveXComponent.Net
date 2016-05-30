using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMQ;
using XComponent.Common.Helper;

namespace ReactiveXComponentTest
{
    [TestFixture]
    public class RabbitMqPublisherTest
    {
        [TestCase()]
        public void Send_Sucess_Test()
        {
            const string exchangeName = "";
            const string routingKey = "";
            var header = Substitute.For<Header>();
            var message = new object();
        
            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            var connection = Substitute.For<IConnection>();
            var model = Substitute.For<IModel>();
            rabbitMqConnection.GetConnection().Returns(connection);
            connection.IsOpen.Returns(true);
            connection.CreateModel().Returns(model);
        
            using (var rabbitPublisher = new RabbitMQPublisher(exchangeName, rabbitMqConnection))
            {
                rabbitPublisher.Send(header, message, routingKey);
            }
        
            model.ReceivedWithAnyArgs().BasicPublish(null, null, null, null);
        }

        //[TestCase()]
        //public void RealSend_Sucess_Test()
        //{
        //    string exchangeName = HashcodeHelper.GetXcHashCode("UserManagement").ToString();
        //    var busInfos = new BusDetails()
        //    {
        //        Host = "localhost",
        //        Username = "test",
        //        Password = "test",
        //        Port = 5672
        //    };
        //    const long userManagement = 1183299321;
        //    const long userSession = 1183299321;
        //    const long eventCode = 153;
        //    const long engineCode = -1805283946;
        //    const string Event = "XComponent.UserManagement.UserObject.GetSessionRequest";
        //    const string visibility = "Public";
        //
        //    var header = new Header()
        //    {
        //        StateMachineCode = userSession,
        //        ComponentCode = userManagement,
        //        EventCode = eventCode,
        //        EngineCode = engineCode,
        //        MessageType = Event,
        //        PublishTopic = visibility
        //    };
        //
        //    var routingKey = "input.0_2_0_10001.engine1.UserManagement.UserManagement";
        //    var message = new XComponent.UserManagement.UserObject.OpenSession();
        //
        //    var rabbitMqConnection = new RabbitMqConnection(busInfos);
        //
        //    using (var rabbitPublisher = new RabbitMQPublisher(exchangeName, rabbitMqConnection))
        //    {
        //        rabbitPublisher.Send(header, message, routingKey);
        //    }
        //}
    }
}
