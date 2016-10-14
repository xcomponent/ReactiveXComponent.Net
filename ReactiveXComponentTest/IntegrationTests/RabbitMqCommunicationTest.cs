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
        private const string User = "guest";
        private const string Password = "guest";
        private const string Host = "127.0.0.1";
        private const int Port = 5672;
        private const string TestMessage = "Test message";
        private const string PrivateCommincationIdentifier = "PrivateCommincationIdentifier";

        private const string ComponentNameA = "ComponentA";
        private const string StateMachineA = "StateMachineA";

        private IXCConfiguration _xcConfiguration;
        private BusDetails _busDetails;
        private string _exchangeName;
        private string _routingKey;
        private string _queueName;
        private IConnection _connection;
        private IModel _channel;
        private string _serialization;
        private ConnectionFactory _connectionFactory;

        [SetUp]
        public void Setup()
        {
            _busDetails = new BusDetails(User, Password, Host, Port);
            _serialization = XCApiTags.Binary;
            var random = new Random();
            _exchangeName = random.Next(100).ToString();
            _routingKey = random.Next(100).ToString();

            _connectionFactory = new ConnectionFactory {
                UserName = _busDetails.Username,
                Password = _busDetails.Password,
                VirtualHost = ConnectionFactory.DefaultVHost,
                HostName = _busDetails.Host,
                Port = _busDetails.Port,
                Protocol = Protocols.DefaultProtocol
            };

            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(_queueName, _exchangeName, _routingKey);

            _xcConfiguration = Substitute.For<IXCConfiguration>();
            _xcConfiguration.GetBusDetails().Returns(_busDetails);
            _xcConfiguration.GetSerializationType().ReturnsForAnyArgs(_serialization);
            _xcConfiguration.GetStateMachineCode(null, null).ReturnsForAnyArgs(0);
            _xcConfiguration.GetComponentCode(null).ReturnsForAnyArgs(Convert.ToInt32(_exchangeName));
            _xcConfiguration.GetPublisherTopic(null, null, 0).ReturnsForAnyArgs(_routingKey);
            _xcConfiguration.GetSubscriberTopic(null, null).ReturnsForAnyArgs(_routingKey);
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMqWithCorrectRoutingKeyAndExchangeName_Test()
        {
            var consumer = new EventingBasicConsumer(_channel);

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

            PublishMessage(ComponentNameA, StateMachineA, Visibility.Public);

            var messageReceived = lockEvent.WaitOne(timeoutReceive);
            
            Check.That(messageReceived).IsTrue();
            Check.That(routingKey).IsEqualTo(_routingKey);
            Check.That(exchangeName).IsEqualTo(_exchangeName);
        }

        [TestCase(Visibility.Private)]
        [TestCase(Visibility.Public)]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMqWithCorrectHeader_Test(Visibility visibility)
        {
            using (var subscriber = CreateSubscriber(ComponentNameA, visibility))
            {
                var expectedHeader = new Header() {
                    StateMachineCode = 0,
                    ComponentCode = Convert.ToInt32(_exchangeName),
                    MessageType = "System.String",
                    EventCode = 0,
                    PublishTopic = visibility == Visibility.Private ? PrivateCommincationIdentifier : string.Empty
                };

                var routingKey = visibility == Visibility.Private ? PrivateCommincationIdentifier : string.Empty;

                IModel channel;
                CreateConsumer(routingKey, out channel);
                
                const int timeoutReceive = 10000;
                var lockEvent = new AutoResetEvent(false);
                StateMachineRefHeader stateMachineRefHeader = null;
                Action<MessageEventArgs> messagedReceivedHandler = args =>
                {
                    stateMachineRefHeader = args.StateMachineRefHeader;
                    lockEvent.Set();
                };

                subscriber.Subscribe(StateMachineA, messagedReceivedHandler);

                PublishMessage(ComponentNameA, StateMachineA, visibility);

                var messageReceived = lockEvent.WaitOne(timeoutReceive);

                Check.That(messageReceived).IsTrue();
                Check.That(MatchesHeader(expectedHeader, stateMachineRefHeader)).IsTrue();
            }

        }

        [TestCase(Visibility.Private)]
        [TestCase(Visibility.Public)]
        public void SendEventWithStateMachineRef_GivenAStateMachineRefAndAMessage_ShouldSendMessageToCorrectStateMachineRef_Test(Visibility visibility)
        {
            var session = CreateSession(visibility);
            var subscriber = session?.CreateSubscriber(ComponentNameA);
            var publisher = session?.CreatePublisher(ComponentNameA);

            var routingKey = visibility == Visibility.Private ? PrivateCommincationIdentifier : string.Empty;

            IModel channel;
            CreateConsumer(routingKey, out channel);

            const int timeoutReceive = 10000;
            var lockEvent = new AutoResetEvent(false);
            StateMachineRefHeader stateMachineRefHeader = null;
            Action<MessageEventArgs> messagedReceivedHandler = args =>
            {
                stateMachineRefHeader = args.StateMachineRefHeader;
                lockEvent.Set();
            };

            subscriber?.Subscribe(StateMachineA, messagedReceivedHandler);
            publisher?.SendEvent(StateMachineA, TestMessage, visibility);

            var messageReceived = lockEvent.WaitOne(timeoutReceive);

            Check.That(messageReceived).IsTrue();

            var firstStateRefHeader = new StateMachineRefHeader()
            {
                StateMachineId = stateMachineRefHeader.StateMachineId,
                AgentId = stateMachineRefHeader.AgentId,
                StateMachineCode = stateMachineRefHeader.StateMachineCode,
                ComponentCode = stateMachineRefHeader.ComponentCode,
                EventCode = stateMachineRefHeader.EventCode,
                MessageType = stateMachineRefHeader.MessageType,
                PublishTopic = stateMachineRefHeader.PublishTopic
            };
            
            publisher?.SendEvent(firstStateRefHeader, 1, visibility);

            var newMessageReceived = lockEvent.WaitOne(timeoutReceive);

            subscriber?.Unsubscribe(StateMachineA, messagedReceivedHandler);

            Check.That(newMessageReceived).IsTrue();

            var secondStateRefHeader = new StateMachineRefHeader()
            {
                StateMachineId = stateMachineRefHeader.StateMachineId,
                AgentId = stateMachineRefHeader.AgentId,
                StateMachineCode = stateMachineRefHeader.StateMachineCode,
                ComponentCode = stateMachineRefHeader.ComponentCode,
                EventCode = stateMachineRefHeader.EventCode,
                MessageType = stateMachineRefHeader.MessageType,
                PublishTopic = stateMachineRefHeader.PublishTopic
            };
            
            Check.That(firstStateRefHeader.AgentId == secondStateRefHeader.AgentId).IsTrue();
            Check.That(firstStateRefHeader.StateMachineId == secondStateRefHeader.StateMachineId).IsTrue();
            Check.That(firstStateRefHeader.ComponentCode == secondStateRefHeader.ComponentCode).IsTrue();
            Check.That(firstStateRefHeader.StateMachineCode == secondStateRefHeader.StateMachineCode).IsTrue();
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMq_Test()
        {
            ISerializer serializer = null;
            switch (_serialization)
            {
                case XCApiTags.Binary:
                    serializer = SerializerFactory.CreateSerializer(SerializationType.Binary);
                    break;
                case XCApiTags.Json:
                    serializer= SerializerFactory.CreateSerializer(SerializationType.Json);
                    break;
            }

            var consumer = new EventingBasicConsumer(_channel);

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

            PublishMessage(ComponentNameA, StateMachineA, Visibility.Public);

            var messageReceived = lockEvent.WaitOne(timeoutReceive);
            var receivedMessage = serializer?.Deserialize(msg);

            Check.That(messageReceived).IsTrue();
            Check.That(receivedMessage).IsEqualTo(TestMessage);
        }

        [TestCase(Visibility.Private)]
        [TestCase(Visibility.Public)]
        public void Subscribe_GivenAcomponentAStateMachineAndACallback_ShouldCreateSubscriptionAndReceiveMessageOnCallback_Test(Visibility visibility)
        {
            using (var subscriber = CreateSubscriber(ComponentNameA, visibility))
            {
                string label = null;
                const int timeoutReceive = 10000;
                var lockEvent = new AutoResetEvent(false);

                Action<MessageEventArgs> messagedReceivedHandler = args => 
                {
                    label = args.MessageReceived as string;
                    lockEvent.Set();
                };

                subscriber.Subscribe(StateMachineA, messagedReceivedHandler);
                PublishMessage(ComponentNameA, StateMachineA, visibility);
               
                var messageReceived = lockEvent.WaitOne(timeoutReceive);

                subscriber.Unsubscribe(StateMachineA, messagedReceivedHandler);

                Check.That(messageReceived).IsTrue();
                Check.That(label).IsNotNull().And.IsNotEmpty().And.Equals(TestMessage);
            }      
        }

        private IXCSession CreateSession(Visibility visibility)
        {
            var privateCommunicationIdentifier = visibility == Visibility.Private ? PrivateCommincationIdentifier : null;
            var rabbitMqConnection = new RabbitMqConnection(_xcConfiguration, privateCommunicationIdentifier);
            return rabbitMqConnection.CreateSession();
        }

        private IXCSubscriber CreateSubscriber(string component, Visibility visibility)
        {
            var session = CreateSession(visibility);
            return session.CreateSubscriber(component);
        }

        private void PublishMessage(string component, string stateMachine, Visibility visibility)
        {
            var session = CreateSession(visibility);

            using (var publisher = session?.CreatePublisher(component))
            {
                publisher?.SendEvent(stateMachine, TestMessage, visibility);  
            }
        }

        private EventingBasicConsumer CreateConsumer(string routingKey, out IModel channel)
        {
            channel = _connection.CreateModel();
            channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
            _queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(_queueName, _exchangeName, routingKey);

            return new EventingBasicConsumer(channel);
        }

        private bool MatchesHeader(Header header, StateMachineRefHeader stateMachineRefHeader)
        {
            return stateMachineRefHeader.StateMachineCode == header.StateMachineCode &&
                    stateMachineRefHeader.ComponentCode == header.ComponentCode &&
                    stateMachineRefHeader.EventCode == header.EventCode &&
                    stateMachineRefHeader.MessageType == header.MessageType &&
                    stateMachineRefHeader.PublishTopic == header.PublishTopic;
        }

        [TearDown]
        public void TearDown()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
