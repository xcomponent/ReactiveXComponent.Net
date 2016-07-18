using System;
using System.IO;
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
using ReactiveXComponent.Serializer;

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
        private string _serialization;

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
            _serialization = "Json";
            var random = new Random();
            _exchangeName = random.Next(100).ToString();
            _routinKey = "routinKey: "+random.Next(100);
            _message = "J'envoie le message n° "+ random.Next(100) +" sur RabbitMq";
            _xcConfiguration = Substitute.For<IXCConfiguration>();
            _xcConfiguration.GetBusDetails().Returns(_busDetails);
            _xcConfiguration.GetSerializationType().ReturnsForAnyArgs(_serialization);
            _xcConfiguration.GetComponentCode(null).ReturnsForAnyArgs(Convert.ToInt32(_exchangeName));
            _xcConfiguration.GetPublisherTopic(null, null, 0).ReturnsForAnyArgs(_routinKey);
            _xcConfiguration.GetSubscriberTopic(null, null).ReturnsForAnyArgs(_routinKey);
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMqWithCorrectRoutingKeyAndExchangeName_Test()
        {
            var consumer = CreateConsumer();

            PublishMessage(Visibility.Public);

            var routingKey = string.Empty;
            var exchangeName = string.Empty;
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
            var expectedHeader = new Header()
            {
                StateMachineCode = 0,
                ComponentCode = Convert.ToInt32(_exchangeName),
                MessageType = "System.String",
                EventCode = 0,
                PublishTopic = visibility == Visibility.Private? "PrivateCommincationIdentifier" : string.Empty
            };

            if (visibility == Visibility.Private)
            {
                _routinKey = "PrivateCommincationIdentifier";
            }
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
            var stateMachineRef = RabbitMqHeaderConverter.ConvertStateMachineRef(headerRepository);

            Check.That(ConatainsHeader(expectedHeader,stateMachineRef)).IsTrue();
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMq_Test()
        {
            SerializerFactory serializerFactory = null;
            switch (_serialization)
            {
                case XCApiTags.Binary:
                    serializerFactory = new SerializerFactory(SerializationType.Binary);
                    break;
                case XCApiTags.Json:
                    serializerFactory = new SerializerFactory(SerializationType.Json);
                    break;
            }
            var serializer = serializerFactory?.CreateSerializer();

            var consumer = CreateConsumer();

            PublishMessage(Visibility.Public);

            Stream msg = null;
            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                msg = new MemoryStream(body);
                lockEvent.Set();
            };
            _channel?.BasicConsume(_queueName, true, consumer);
            if (!lockEvent.WaitOne(timeoutReceive))
            {
                throw new Exception("Message not received");
            }
            var contenu = serializer?.Deserialize(msg);

            Check.That(contenu).IsEqualTo(_message);
        }

        [TestCase(Visibility.Private)]
        [TestCase(Visibility.Public)]
        public void Subscribe_GivenAcomponentAStateMachineAndACallback_ShouldCreateSubscriptionAndReceiveMessageOnCallback_Test(Visibility visibility)
        {
            var subscirber = CreateSubscriber(visibility);
            const string component = "Component";
            const string stateMachine = "stateMachine";

            subscirber.Subscribe(component, stateMachine, MessageReceivedUpdated);

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

            Check.That(label).IsNotEmpty().And.Contains("J'envoie le message n° ").And.Contains("sur RabbitMq");
                
        }

        private void MessageReceivedUpdated(MessageEventArgs busEvent)
        {
            if (MessageReceived == null) return;
            var publicMember = busEvent.MessageReceived as string;
            if (publicMember != null) MessageReceived(publicMember);
        }


        private IXCSession CreateSession(Visibility visibility)
        {
            var privateCommunicationIdentifier = visibility == Visibility.Private ? "PrivateCommincationIdentifier" : null;
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

        private bool ConatainsHeader(Header header, StateMachineRef stateMachineRef)
        {
            return stateMachineRef.StateMachineCode == header.StateMachineCode && 
                    stateMachineRef.ComponentCode == header.ComponentCode && 
                    stateMachineRef.EventCode == header.EventCode && 
                    stateMachineRef.MessageType == header.MessageType && 
                    stateMachineRef.PublishTopic == header.PublishTopic;
        }

        [TearDown]
        public void TearDown()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
