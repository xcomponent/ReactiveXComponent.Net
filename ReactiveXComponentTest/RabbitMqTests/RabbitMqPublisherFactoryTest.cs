using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest
{
    [TestFixture]
    [Category("Unit Tests")]
    class RabbitMqPublisherFactoryTest
    {
        private RabbitMqPublisher _rabbitMqPublisher;
        private Header _header;
        private object _message;
        private string _routingKey;
        private IModel _model;
         
        [SetUp]
        public void Setup()
        {
            _routingKey = "";
            _header = Substitute.For<Header>();
            _message = new object();

            var rabbitMqConnection = Substitute.For<IRabbitMqConnection>();
            var connection = Substitute.For<IConnection>();
            _model = Substitute.For<IModel>();
            rabbitMqConnection.GetConnection().Returns(connection);
            connection.IsOpen.Returns(true);
            connection.CreateModel().Returns(_model);

            var rabbitMqPublisherFactory = new RabbitMqPublisherFactory(rabbitMqConnection);
            _rabbitMqPublisher = rabbitMqPublisherFactory.Create("") as RabbitMqPublisher;
        }

        [TestCase()]
        public void Send_Sucess_Test()
        {
            _rabbitMqPublisher.Send(_header, _message, _routingKey);
            _model.ReceivedWithAnyArgs().BasicPublish(null, null, null, null);
        }

        [TearDown]
        public void TearDown()
        {
            _rabbitMqPublisher.Dispose();
            _model.Dispose();
        }
    }
}
