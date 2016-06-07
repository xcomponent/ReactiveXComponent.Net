using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqConsumerFactoryTest
    {
        private SingleKeyRabbitMqConsumer _rabbitMqConsumer;
        private IConnection _connection;

        [SetUp]
        public void Setup()
        {
            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            _connection = Substitute.For<IConnection>();
            var model = Substitute.For<IModel>();
            rabbitMqConnection.GetConnection().Returns(_connection);
            _connection.IsOpen.Returns(true);
            _connection.CreateModel().Returns(model);
            var queue = new QueueDeclareOk("", 0, 0);
            model.QueueDeclare().Returns(queue);

            var rabbitMqConsumerFactory = new SingleKeyRabbitMqConsumerFactory(rabbitMqConnection);
            _rabbitMqConsumer = rabbitMqConsumerFactory.Create("", "") as SingleKeyRabbitMqConsumer;
        }

        [TestCase()]
        public void Start_GivenNoArgs_ShouldCreateACommunicationChannel_Test()
        {
            _rabbitMqConsumer.Start();
            _connection.ReceivedWithAnyArgs(2).CreateModel();
        }
        
        [TestCase()]
        public void Stop_GivenNoArgs_ShouldStopCommunicationAndCloseChanels_Test()
        {
            _rabbitMqConsumer.Start();
            _rabbitMqConsumer.Stop();
            var isStarted = _rabbitMqConsumer.IsStarted;
        
            Assert.IsFalse(isStarted);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
        }
    }
}
