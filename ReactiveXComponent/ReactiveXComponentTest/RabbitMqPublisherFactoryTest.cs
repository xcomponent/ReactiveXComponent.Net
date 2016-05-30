using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest
{
    [TestFixture]
    class RabbitMqPublisherFactoryTest
    {
        [TestCase()]
        public void Send_Sucess_Test()
        {
            const string routingKey = "";
            var header = Substitute.For<Header>();
            var message = new object();
        
            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            var connection = Substitute.For<IConnection>();
            var model = Substitute.For<IModel>();
            rabbitMqConnection.GetConnection().Returns(connection);
            connection.IsOpen.Returns(true);
            connection.CreateModel().Returns(model);

            var rabbitPublisherFactory = new RabbitMqPublisherFactory(rabbitMqConnection);
            var rabbitPublisher = rabbitPublisherFactory.Create("");
            rabbitPublisher.Send(header, message, routingKey);
        
            model.ReceivedWithAnyArgs().BasicPublish(null, null, null, null);
        }
    }
}
