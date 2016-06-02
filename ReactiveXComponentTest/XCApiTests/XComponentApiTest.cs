using System;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;
using ReactiveXComponent.XCApi;

namespace ReactiveXComponentTest.XCApiTests
{
    [TestFixture]
    class XComponentApiTest
    {
        [TestCase()]
        public void SendEvent_Sucess_Test()
        {
            var engine = String.Empty;
            var component = "ExchangeManager";
            var stateMachine = "ExchangeManager";
            var eventCode = 102;
            var messageType = String.Empty;
            object message = new object();
            var visibility = Visibility.Private;

            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            var connection = Substitute.For<IConnection>();
            rabbitMqConnection.GetConnection().Returns(connection);
            connection.IsOpen.Returns(true);
            var publisherFactory = Substitute.For<IRabbitMqPublisherFactory>();
            var publisher = Substitute.For<RabbitMqPublisher>(String.Empty, rabbitMqConnection);
            publisherFactory.Create(component).Returns(publisher);
            var consumerFactory = Substitute.For<IRabbitMqConsumerFactory>();
            var parser = Substitute.For<DeploymentParser>("XCApiTests//PerseusApi.xcApi");

            using (var xcApi = new XComponentApi(publisherFactory, consumerFactory, rabbitMqConnection, parser))
            {
                xcApi.SendEvent(engine, component, stateMachine, eventCode, messageType, message, visibility);
            }

            publisher.ReceivedWithAnyArgs().Send(null, null, null);
        }

        //[TestCase()]
        //public void AddCallBack_Sucess_Test()
        //{
        //    var component = "CheckOrder";
        //    var stateMachine = "CheckComponent";
        //    Action<MessageEventArgs> callback = new Action<MessageEventArgs>(x => { x = null; });
        //
        //    var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
        //    var connection = Substitute.For<IConnection>();
        //    rabbitMqConnection.GetConnection().Returns(connection);
        //    connection.IsOpen.Returns(true);
        //    var publisherFactory = Substitute.For<IRabbitMqPublisherFactory>();
        //    var consumerFactory = Substitute.For<IRabbitMqConsumerFactory>();
        //    var consumer = Substitute.For<IConsumer>();
        //    consumerFactory.Create(component, stateMachine).Returns(consumer);
        //    var parser = Substitute.For<DeploymentParser>("XCApiTests//PerseusApi.xcApi");
        //
        //    using (var xcApi = new XComponentApi(publisherFactory, consumerFactory, rabbitMqConnection, parser))
        //    {
        //        xcApi.AddCallback(component, stateMachine, callback);
        //    }
        //
        //    consumer.ReceivedWithAnyArgs().Start();
        //}

    }
}
