using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest
{
    [TestFixture]
    class RabbitMqConsumerFactoryTest
    {
        [TestCase()]
        public void StartExchange_Sucess_Test()
        {
            const string exchangeName = "";
            const string routingKey = "";
        
            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            var connection = Substitute.For<IConnection>();
            var model = Substitute.For<IModel>();
            rabbitMqConnection.GetConnection().Returns(connection);
            connection.IsOpen.Returns(true);
            connection.CreateModel().Returns(model);
            var queue = new QueueDeclareOk("", 0, 0);
            model.QueueDeclare().Returns(queue);
        
            var rabbitMqConsumerFactory = new SingleKeyRabbitMQConsumerFactory(rabbitMqConnection);
        
            var rabbitMqConsumer = rabbitMqConsumerFactory.Create("","") as SingleKeyRabbitMqConsumer;
            var isStarted = rabbitMqConsumer.IsStarted;
            rabbitMqConsumer.Start();
            isStarted = rabbitMqConsumer.IsStarted;
        
            Assert.IsTrue(isStarted);
        }
        
        [TestCase()]
        public void EndExchange_Sucess_Test()
        {
            const string exchangeName = "";
            const string routingKey = "";
        
            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            var connection = Substitute.For<IConnection>();
            var model = Substitute.For<IModel>();
            rabbitMqConnection.GetConnection().Returns(connection);
            connection.IsOpen.Returns(true);
            connection.CreateModel().Returns(model);
            var queue = new QueueDeclareOk("", 0, 0);
            model.QueueDeclare().Returns(queue);

            var rabbitMqConsumerFactory = new SingleKeyRabbitMQConsumerFactory(rabbitMqConnection);

            var rabbitMqConsumer = rabbitMqConsumerFactory.Create("", "") as SingleKeyRabbitMqConsumer;
            var isStarted = rabbitMqConsumer.IsStarted;
            rabbitMqConsumer.Start();
            rabbitMqConsumer.Stop();
            isStarted = rabbitMqConsumer.IsStarted;
        
            Assert.IsFalse(isStarted);
        }

    }
}
