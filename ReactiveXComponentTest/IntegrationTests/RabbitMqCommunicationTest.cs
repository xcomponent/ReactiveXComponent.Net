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
using ReactiveXComponent.Connection;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest.IntegrationTests
{
    [TestFixture]
    [Category("Integration Tests")]
    public class RabbitMqCommunicationTest
    {
        private IXCConfiguration _xcConfiguration;
        private BusDetails _busDetails;
        private string _exchangeName;
        private string _routinKey;
        private string _message;
        private string _queueName;
        private IConnection _connection;
        private IModel _channel;

        private event Action<string> MessageReceived;

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
            var random = new Random();
            _exchangeName = random.Next(100).ToString();
            _routinKey = "routinKey: "+random.Next(100);
            _message = "J'envoie le message n° "+ random.Next(100) +" sur RabbitMq";
            _xcConfiguration = Substitute.For<IXCConfiguration>();
            _xcConfiguration.GetBusDetails().Returns(_busDetails);
            _xcConfiguration.GetComponentCode(null).ReturnsForAnyArgs(Convert.ToInt32(_exchangeName));
            _xcConfiguration.GetPublisherTopic(null, null, 0).ReturnsForAnyArgs(_routinKey);
            _xcConfiguration.GetSubscriberTopic(null, null).ReturnsForAnyArgs(_routinKey);
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMqWithCorrectRoutingKeyAndExchangeName_Test()
        {
            var consumer = CreateConsumer();

            PublishMessage(Visibility.Public);

            string routingKey = string.Empty;
            string exchangeName = string.Empty;
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            consumer.Received += (model, ea) =>
            {
                routingKey = ea.RoutingKey;
                exchangeName = ea.Exchange;
                lockEvent.Set();
            };
            _channel?.BasicConsume(_queueName, true, consumer);
            
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }
            Check.That(routingKey).IsEqualTo(_routinKey);
            Check.That(exchangeName).IsEqualTo(_exchangeName);
        }

        [TestCase(Visibility.Private)]
        [TestCase(Visibility.Public)]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMqWithCorrectHeader_Test(Visibility visibility)
        {
            var expectedHeader = new Header
            {
                StateMachineCode = 0,
                ComponentCode = Convert.ToInt32(_exchangeName),
                MessageType = "System.String",
                EventCode = 0,
                PublishTopic = visibility == Visibility.Private? "PrivateCommincationIdentifier" : string.Empty
            };

            var consumer = CreateConsumer();

            PublishMessage(visibility);

            IBasicProperties basicProperties = null;
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            consumer.Received += (model, ea) =>
            {
                basicProperties = ea.BasicProperties;
                lockEvent.Set();
            };
            _channel?.BasicConsume(_queueName, true, consumer);
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }
            var headerRepository = basicProperties?.Headers;
            var header = RabbitMqHeaderConverter.ConvertHeader(headerRepository);
            Check.That(header).IsEqualTo(expectedHeader);
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMq_Test()
        {
            var consumer = CreateConsumer();

            PublishMessage(Visibility.Public);

            byte[] msg = new byte[] {};
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                msg = body;
                lockEvent.Set();
            };
            _channel?.BasicConsume(_queueName, true, consumer);
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }
            var binaryFormater = new BinaryFormatter();
            var contenu = binaryFormater.Deserialize(new MemoryStream(msg)).ToString();
            Check.That(contenu).IsEqualTo(_message);
        }

        [TestCase(Visibility.Private)]
        [TestCase(Visibility.Public)]
        public void AddCallback_GivenAcomponentAStateMachineAndACallback_ShouldCreateSubscriptionAndReceiveMessageOnCallback_Test(Visibility visibility)
        {
            var subscirber = CreateSubscriber(visibility);
            const string component = "Component";
            const string stateMachine = "stateMachine";

            subscirber.AddCallback(component, stateMachine, MessageReceivedUpdated);

            PublishMessage(visibility);

            string label = null;
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            
            MessageReceived += instance =>
            {
                label = instance;
                lockEvent.Set();
            };
            
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }

            Check.That(label).IsNotEmpty();
                
        }

        private void MessageReceivedUpdated(MessageEventArgs busEvent)
        {
            if (MessageReceived == null) return;
            var publicMember = busEvent.MessageReceived as string;
            if (publicMember != null) MessageReceived(publicMember);
        }


        private IXCSession CreateSession(Visibility visibility)
        {
            var privateCommunicationIdentifier = visibility == Visibility.Private ? "PrivateCommincationIdentifier" : string.Empty;
            var rabbitMqConnection = new RabbitMqConnection(_xcConfiguration, privateCommunicationIdentifier);
            return rabbitMqConnection.CreateSession();
        }
        private IXCSubscriber CreateSubscriber(Visibility visibility)
        {
            var session = CreateSession(visibility);
            return session.CreateSubscriber();
        }

        private void PublishMessage(Visibility visibility)
        {
            var session = CreateSession(visibility);
            const string component = "Component";
            const string stateMachine = "stateMachine";

            using (var publisher = session?.CreatePublisher(component))
            {
                publisher?.SendEvent(stateMachine, _message, visibility);  
            }
        }

        private EventingBasicConsumer CreateConsumer()
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

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(_queueName, _exchangeName, _routinKey);
            var consumer = new EventingBasicConsumer(_channel);

            return consumer;
        }

        [TearDown]
        public void TearDown()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
