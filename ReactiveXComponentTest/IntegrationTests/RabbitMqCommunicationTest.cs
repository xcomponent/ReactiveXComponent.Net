using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.IntegrationTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqCommunicationTest
    {
        private IXCConfiguration _xcConfiguration;
        private BusDetails _busDetails;
        private string _exchangeName;
        private string _routinKey;
        private string _message;
        private string _queueName;

        [SetUp]
        public void Setup()
        {
            _busDetails = new BusDetails
            {
                Host = "127.0.0.1",
                Password = "guest",
                Port = 5672,
                Username = "guest"
            };
            _exchangeName = "3";
            _routinKey = "Component.Test.Envoie.Message";
            _message = "J'envoie un message sur RabbitMq";
            _xcConfiguration = Substitute.For<IXCConfiguration>();
            _xcConfiguration.GetBusDetails().Returns(_busDetails);
            _xcConfiguration.GetComponentCode(null).ReturnsForAnyArgs(Convert.ToInt32(_exchangeName));
            _xcConfiguration.GetPublisherTopic(null, null, 0).ReturnsForAnyArgs(_routinKey);
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMqWithCoorectRoutingKeyAndExchangeName_Test()
        {
            var channel = CreateConsumerChannel();
            var consumer = new EventingBasicConsumer(channel);

            PublishMessage();

            string routingKey = string.Empty;
            string exchangeName = string.Empty;
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                consumer.Received += (model, ea) =>
                {
                    routingKey = ea.RoutingKey;
                    exchangeName = ea.Exchange;
                    lockEvent.Set();
                };
                channel?.BasicConsume(queue: _queueName, noAck: true, consumer: consumer);
            });
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }
            else
            {
                Check.That(routingKey).IsEqualTo(_routinKey);
                Check.That(exchangeName).IsEqualTo(_exchangeName);
            }

        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMqWithCorrectHeader_Test()
        {

            var expectedHeader = new Header
            {
                StateMachineCode = 0,
                ComponentCode = 3,
                MessageType = "System.String",
                EventCode = 0,
                PublishTopic = string.Empty
            };

            var channel = CreateConsumerChannel();
            var consumer = new EventingBasicConsumer(channel);

            PublishMessage();

            IBasicProperties basicProperties = null;
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                consumer.Received += (model, ea) =>
                {
                    basicProperties = ea.BasicProperties;
                    lockEvent.Set();
                };
                channel?.BasicConsume(queue: _queueName, noAck: true, consumer: consumer);
            });
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }
            else
            {
                var headerRepository = basicProperties?.Headers;
                var header = RabbitMqHeaderConverter.ConvertHeader(headerRepository);
                Check.That(header).IsEqualTo(expectedHeader);
            }
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMq_Test()
        {
            
            var channel = CreateConsumerChannel();
            var consumer = new EventingBasicConsumer(channel);

            PublishMessage();

            byte[] msg = new byte[] {};
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    msg = body;
                    lockEvent.Set();
                };
                channel?.BasicConsume(queue: _queueName, noAck: true, consumer: consumer);
            });
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }
            else
            {
                var binaryFormater = new BinaryFormatter();
                var contenu = binaryFormater.Deserialize(new MemoryStream(msg)).ToString();
                Check.That(contenu).IsEqualTo(_message);
            }
        }

        private void PublishMessage()
        {
            var rabbitMqConnection = new RabbitMqConnection(_xcConfiguration);
            var session = rabbitMqConnection.CreateSession();

            using (var publisher = session?.CreatePublisher("Component"))
            {
                publisher?.SendEvent("stateMachine", _message, Visibility.Public);  
            }
        }

        private IModel CreateConsumerChannel()
        {
            var factory = new ConnectionFactory
            {
                UserName = _busDetails.Username,
                Password = _busDetails.Password,
                VirtualHost = ConnectionFactory.DefaultVHost,
                HostName = _busDetails.Host,
                Port = _busDetails.Port,
                Protocol = Protocols.DefaultProtocol
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
            _queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routinKey);
            return channel;
        }
    }
}
