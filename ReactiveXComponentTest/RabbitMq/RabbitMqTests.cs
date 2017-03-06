using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponentTest.RabbitMq
{
    [TestFixture]
    public class RabbitMqTests
    {
        private const string User = "guest";
        private const string Password = "guest";
        private const string Host = "127.0.0.1";
        private const int Port = 5672;
        private const string TestMessage = "Test message";
        private const string PrivateCommincationIdentifier = "PrivateCommincationIdentifier";

        private const string ComponentNameA = "ComponentA";
        private const string StateMachineA = "StateMachineA";
        private const string QueueName = "queue";
        private const string ExchangeName = "101";
        private const string PublicRoutingKey = "202";
        private const string Serialization = XCApiTags.Binary;
        private const string SnapshotSerialization = XCApiTags.Json;
        private readonly BusDetails _busDetails = new BusDetails(User, Password, Host, Port);

        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void PublisherSendEventTest(bool isPrivate, bool withStateMachineRef)
        {
            var topicToUse = isPrivate ? PrivateCommincationIdentifier : string.Empty;
            var routingKeyToUse = isPrivate ? PrivateCommincationIdentifier : PublicRoutingKey;

            var configuration = Substitute.For<IXCConfiguration>();
            configuration.GetBusDetails().Returns(_busDetails);
            configuration.GetSerializationType().ReturnsForAnyArgs(Serialization);
            configuration.GetStateMachineCode(ComponentNameA, StateMachineA).Returns(0);
            configuration.GetComponentCode(ComponentNameA).Returns(Convert.ToInt32(ExchangeName));
            configuration.GetPublisherTopic(ComponentNameA, StateMachineA).Returns(routingKeyToUse);
            configuration.GetPublisherTopic(Convert.ToInt32(ExchangeName), 0).Returns(routingKeyToUse);
            configuration.GetSubscriberTopic(ComponentNameA, StateMachineA).Returns(PublicRoutingKey);

            var connection = Substitute.For<IConnection>();
            var channel = Substitute.For<IModel>();
            channel.CreateBasicProperties().ReturnsForAnyArgs(new BasicProperties());
            connection.CreateModel().Returns(channel);
            connection.IsOpen.Returns(true);
            channel.WhenForAnyArgs(x => x.ExchangeDeclare(null, null, true, true, null)).DoNotCallBase();
            var queueDeclareOk = new QueueDeclareOk(QueueName, uint.MaxValue, uint.MaxValue);
            channel.QueueDeclare().ReturnsForAnyArgs(queueDeclareOk);
            channel.WhenForAnyArgs(x => x.BasicConsume(QueueName, false, null)).DoNotCallBase();
            channel.WhenForAnyArgs(x => x.QueueBind(QueueName, null, null, null)).DoNotCallBase();

            var routingKey = string.Empty;
            var exchangeName = string.Empty;
            long componentCode = -1;
            long stateMachineCode = -1;
            long stateMachineId = -1;
            var topic = string.Empty;
            const int timeoutReceive = 10000;
            var stateMachineIdToSend = 1;

            using (var publisher = new RabbitMqPublisher(ComponentNameA, configuration, connection, GetSerializer(Serialization), PrivateCommincationIdentifier))
            using (var messageSentEvent = new AutoResetEvent(false))
            {
                var publishAction = new Action<string, string, bool, IBasicProperties, byte[]>((exchange, routingkey, mandatory, properties, bytes) =>
                {
                    exchangeName = exchange;
                    routingKey = routingkey;
                    var stateMachineRefHeader = RabbitMqHeaderConverter.ConvertStateMachineRefHeader(properties.Headers);
                    componentCode = stateMachineRefHeader.ComponentCode;
                    stateMachineCode = stateMachineRefHeader.StateMachineCode;
                    stateMachineId = stateMachineRefHeader.StateMachineId;
                    topic = stateMachineRefHeader.PublishTopic;

                    messageSentEvent.Set();
                });
                
                channel.WhenForAnyArgs(x => x.BasicPublish(null, null, false, null, null)).Do(callInfo =>
                {
                    var args = callInfo.Args();
                    publishAction((string)args[0], (string)args[1], (bool)args[2],
                        (BasicProperties)args[3], (byte[])args[4]);
                });

                if (withStateMachineRef)
                {
                    var stateMachineRef = new StateMachineRefHeader() {
                        ComponentCode = configuration.GetComponentCode(ComponentNameA),
                        StateMachineCode = configuration.GetStateMachineCode(ComponentNameA, StateMachineA),
                        StateMachineId = stateMachineIdToSend,
                        PublishTopic = topicToUse
                    };

                    publisher.SendEvent(stateMachineRef, TestMessage, isPrivate ? Visibility.Private : Visibility.Public);
                }
                else
                {
                    publisher.SendEvent(StateMachineA, TestMessage, isPrivate ? Visibility.Private : Visibility.Public);
                }

                var messageSent = messageSentEvent.WaitOne(timeoutReceive);

                Check.That(messageSent).IsTrue();
                Check.That(routingKey).IsEqualTo(routingKeyToUse);
                Check.That(exchangeName).IsEqualTo(ExchangeName);
                Check.That(topic).IsEqualTo(topicToUse);

                if (withStateMachineRef)
                {
                    Check.That(componentCode).IsEqualTo(configuration.GetComponentCode(ComponentNameA));
                    Check.That(stateMachineCode).IsEqualTo(configuration.GetStateMachineCode(ComponentNameA, StateMachineA));
                    Check.That(stateMachineId).IsEqualTo(stateMachineIdToSend);
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SubscriberTest(bool isPrivate)
        {
            var topicToUse = isPrivate ? PrivateCommincationIdentifier : string.Empty;
            var routingKeyToUse = isPrivate ? PrivateCommincationIdentifier : PublicRoutingKey;
            EventingBasicConsumer consumer = null;

            var configuration = Substitute.For<IXCConfiguration>();
            configuration.GetBusDetails().Returns(_busDetails);
            configuration.GetSerializationType().ReturnsForAnyArgs(Serialization);
            configuration.GetStateMachineCode(ComponentNameA, StateMachineA).Returns(2);
            configuration.GetComponentCode(ComponentNameA).Returns(Convert.ToInt32(ExchangeName));
            configuration.GetSubscriberTopic(ComponentNameA, StateMachineA).Returns(routingKeyToUse);

            var connection = Substitute.For<IConnection>();
            var channel = Substitute.For<IModel>();
            channel.CreateBasicProperties().ReturnsForAnyArgs(new BasicProperties());
            connection.CreateModel().Returns(channel);
            connection.IsOpen.Returns(true);
            channel.WhenForAnyArgs(x => x.ExchangeDeclare(null, null, true, true, null)).DoNotCallBase();
            var queueDeclareOk = new QueueDeclareOk(QueueName, uint.MaxValue, uint.MaxValue);
            channel.QueueDeclare().ReturnsForAnyArgs(queueDeclareOk);
            
            var consumeAction = new Action<string, bool, IBasicConsumer>((queueName, noAck, aconsumer) =>
            {
                consumer = (EventingBasicConsumer)aconsumer;
            });
            
            channel.WhenForAnyArgs(x => x.BasicConsume("", false, "", false, false, null, null)).Do(callInfo =>
            {
                var args = callInfo.Args();
                if (args.Length == 7)
                {
                    consumeAction((string)args[0], (bool)args[1], (EventingBasicConsumer)callInfo.Args()[6]);
                }
            });

            channel.WhenForAnyArgs(x => x.QueueBind(QueueName, null, null, null)).DoNotCallBase();

            const int receptionTimeout = 10000;
            const string messageType = "System.String";
            long componentCode = 1;
            long stateMachineCode = 2;
            
            var receivedMessageType = string.Empty;
            var receivedMessage = string.Empty;

            using (var subscriber = new RabbitMqSubscriber(ComponentNameA, configuration, connection, GetSerializer(Serialization), PrivateCommincationIdentifier))
            using (var messageReceivedEvent = new AutoResetEvent(false))
            {
                var messageReceptionHandler = new Action<MessageEventArgs>(args =>
                {
                    if (args.StateMachineRefHeader.ComponentCode == componentCode
                        && args.StateMachineRefHeader.StateMachineCode == stateMachineCode)
                    {
                        receivedMessageType = args.StateMachineRefHeader.MessageType;

                        if (receivedMessageType == messageType)
                        {
                            receivedMessage = (string) args.MessageReceived;
                        }

                        messageReceivedEvent.Set();
                    }
                });

                subscriber.Subscribe(StateMachineA, messageReceptionHandler);

                var stateMachineRef = new StateMachineRefHeader()
                {
                    ComponentCode = componentCode,
                    StateMachineCode = stateMachineCode,
                    MessageType = messageType,
                    PublishTopic = topicToUse
                };

                var basicProperties = new BasicProperties()
                {
                    Headers = RabbitMqHeaderConverter.CreateHeaderFromStateMachineRefHeader(stateMachineRef, IncomingEventType.Transition) 
                };

                var stream = new MemoryStream();
                GetSerializer(Serialization).Serialize(stream, TestMessage);
                var message = stream.ToArray();
                consumer.HandleBasicDeliver(consumer.ConsumerTag, ulong.MaxValue, false, ExchangeName, routingKeyToUse, basicProperties, message);

                var messageReceived = messageReceivedEvent.WaitOne(receptionTimeout);

                Check.That(messageReceived).IsTrue();
                Check.That(receivedMessageType).IsEqualTo(messageType);
                Check.That(receivedMessage).IsEqualTo(TestMessage);

                subscriber.Unsubscribe(StateMachineA, messageReceptionHandler);
            }
        }

        [Test]
        public void SnapshotTest()
        {
            EventingBasicConsumer consumer = null;

            var configuration = Substitute.For<IXCConfiguration>();
            configuration.GetBusDetails().Returns(_busDetails);
            configuration.GetSerializationType().ReturnsForAnyArgs(XCApiTags.Json);
            configuration.GetStateMachineCode(ComponentNameA, StateMachineA).Returns(2);
            configuration.GetComponentCode(ComponentNameA).Returns(Convert.ToInt32(ExchangeName));
            configuration.GetSubscriberTopic(ComponentNameA, StateMachineA).Returns(PublicRoutingKey);

            var connection = Substitute.For<IConnection>();
            var channel = Substitute.For<IModel>();
            channel.CreateBasicProperties().ReturnsForAnyArgs(new BasicProperties());
            connection.CreateModel().Returns(channel);
            connection.IsOpen.Returns(true);
            channel.WhenForAnyArgs(x => x.ExchangeDeclare(null, null, true, true, null)).DoNotCallBase();
            var queueDeclareOk = new QueueDeclareOk(QueueName, uint.MaxValue, uint.MaxValue);
            channel.QueueDeclare().ReturnsForAnyArgs(queueDeclareOk);

            var consumeAction = new Action<string, bool, IBasicConsumer>((queueName, noAck, aconsumer) => 
            {
                consumer = (EventingBasicConsumer)aconsumer;
            });

            channel.WhenForAnyArgs(x => x.BasicConsume("", false, "", false, false, null, null)).Do(callInfo => 
            {
                var args = callInfo.Args();
                if (args.Length == 7)
                {
                    consumeAction((string)args[0], (bool)args[1], (EventingBasicConsumer)callInfo.Args()[6]);
                }
            });

            var snapshotReplyTopic = string.Empty;

            channel.WhenForAnyArgs(x => x.QueueBind(QueueName, null, null, null)).Do(callInfo =>
            {
                var args = callInfo.Args();
                snapshotReplyTopic = (string)args[2];
            });

            channel.WhenForAnyArgs(x => x.BasicPublish(null, null, false, null, null)).DoNotCallBase();

            const int receptionTimeout = 10000;
            long componentCode = 1;
            long stateMachineCode = 2;

            using (var publisher = new RabbitMqPublisher(ComponentNameA, configuration, connection, GetSerializer(SnapshotSerialization), PrivateCommincationIdentifier))
            using (var snapshotReceivedEvent = new AutoResetEvent(false))
            {
                List<MessageEventArgs> snapshotInstances = null;

                var snapshotHandler = new Action<List<MessageEventArgs>>(instances =>
                {
                    snapshotInstances = instances;
                    snapshotReceivedEvent.Set();
                });

                publisher.GetSnapshotAsync(StateMachineA, snapshotHandler);

                var stateMachineRef = new StateMachineRefHeader() {
                    ComponentCode = componentCode,
                    StateMachineCode = stateMachineCode,
                    PublishTopic = PublicRoutingKey
                };

                var stateMachineInstances = new List<StateMachineInstance>()
                {
                    new StateMachineInstance()
                    {
                        ComponentCode = componentCode,
                        StateMachineCode = stateMachineCode,
                        PublicMember = TestMessage
                    }
                };
                
                var message = SerializeSnapshotInstances(stateMachineInstances);

                var basicProperties = new BasicProperties() 
                {
                    Headers = RabbitMqHeaderConverter.CreateHeaderFromStateMachineRefHeader(stateMachineRef, IncomingEventType.Snapshot)
                };

                consumer.HandleBasicDeliver(consumer.ConsumerTag, ulong.MaxValue, false, ExchangeName, snapshotReplyTopic, basicProperties, message);

                var messageReceived = snapshotReceivedEvent.WaitOne(receptionTimeout);

                Check.That(messageReceived).IsTrue();
                Check.That(snapshotInstances.Count).IsEqualTo(1);
                Check.That(snapshotInstances.FirstOrDefault()?.MessageReceived).IsInstanceOf<string>();
                Check.That((string)(snapshotInstances.FirstOrDefault()?.MessageReceived)).IsEqualTo(TestMessage);
            }
        }


        private ISerializer GetSerializer(string serializer)
        {
            switch (serializer)
            {
                    case XCApiTags.Binary:
                        return new BinarySerializer();
                    case XCApiTags.Json:
                        return new ReactiveXComponent.Serializer.JsonSerializer();
                    case XCApiTags.Bson:
                        return new BsonSerializer();
                default: 
                    return new BinarySerializer();
            }
        }
        private byte[] SerializeSnapshotInstances(List<StateMachineInstance> instances)
        {
            var serializedInstances = JsonConvert.SerializeObject(instances);
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(serializedInstances);
            streamWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            byte[] compressedBytes = null;

            using (var compressed = new MemoryStream())
            {
                using (var compressor = new GZipStream(compressed, CompressionMode.Compress))
                {
                    stream.CopyTo(compressor);
                }

                compressedBytes = compressed.ToArray();
            }

            var base64String = Convert.ToBase64String(compressedBytes, Base64FormattingOptions.None);
            var snapshotItems = new SnapshotItems() 
            {
                Items = base64String
            };

            var serializedMessage = JsonConvert.SerializeObject(snapshotItems);
            var messageStream = new MemoryStream();
            GetSerializer(SnapshotSerialization).Serialize(messageStream, serializedMessage);
            messageStream.Flush();
            messageStream.Seek(0, SeekOrigin.Begin);

            return messageStream.ToArray();
        }
    }
}
