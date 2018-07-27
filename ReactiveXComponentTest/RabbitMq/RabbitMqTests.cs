using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            string stateMachineId = null;
            var topic = string.Empty;
            const int timeoutReceive = 10000;
            string stateMachineIdToSend = "1";

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
                    topic = stateMachineRefHeader.PrivateTopic;

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
                        PrivateTopic = topicToUse
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
            int componentCode = 1;
            int stateMachineCode = 2;
            int eventCode = 1;
            
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
                    PrivateTopic = topicToUse
                };

                var basicProperties = new BasicProperties()
                {
                    Headers = RabbitMqHeaderConverter.CreateHeaderFromStateMachineRefHeader(stateMachineRef, IncomingEventType.Transition, eventCode) 
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
        public void SubscriberHeaderFieldsTest()
        {
            EventingBasicConsumer consumer = null;

            var configuration = Substitute.For<IXCConfiguration>();
            configuration.GetBusDetails().Returns(_busDetails);
            configuration.GetSerializationType().ReturnsForAnyArgs(Serialization);
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

            channel.WhenForAnyArgs(x => x.BasicConsume("", false, "", false, false, null, null)).Do(callInfo => {
                var args = callInfo.Args();
                if (args.Length == 7)
                {
                    consumeAction((string)args[0], (bool)args[1], (EventingBasicConsumer)callInfo.Args()[6]);
                }
            });

            channel.WhenForAnyArgs(x => x.QueueBind(QueueName, null, null, null)).DoNotCallBase();

            const int receptionTimeout = 10000;
            const string messageTypeSent = "System.String";
            const int componentCodeSent = 1;
            const int stateMachineCodeSent = 2;
            const string stateMachineIdSent = "81";
            const int stateCodeSent = -2147483648;
            const string errorMessageSent = "Some error message";
            const int eventCode = 1;

            var componentCodeReceived = 0;
            var stateMachineCodeReceived = 0;
            string stateMachineIdReceived = null;
            var stateCodeReceived = 0;
            var errorMessageReceived = string.Empty;
            

            var receivedMessageType = string.Empty;

            using (var subscriber = new RabbitMqSubscriber(ComponentNameA, configuration, connection, GetSerializer(Serialization), PrivateCommincationIdentifier))
            using (var messageReceivedEvent = new AutoResetEvent(false))
            {
                var messageReceptionHandler = new Action<MessageEventArgs>(args => 
                {
                    if (args.StateMachineRefHeader.ComponentCode == componentCodeSent
                        && args.StateMachineRefHeader.StateMachineCode == stateMachineCodeSent)
                    {
                        receivedMessageType = args.StateMachineRefHeader.MessageType;

                        if (receivedMessageType == messageTypeSent)
                        {
                            componentCodeReceived = args.StateMachineRefHeader.ComponentCode;
                            stateMachineCodeReceived = args.StateMachineRefHeader.StateMachineCode;
                            stateMachineIdReceived = args.StateMachineRefHeader.StateMachineId;
                            stateCodeReceived = args.StateMachineRefHeader.StateCode;
                            errorMessageReceived = args.StateMachineRefHeader.ErrorMessage;
                        }

                        messageReceivedEvent.Set();
                    }
                });

                subscriber.Subscribe(StateMachineA, messageReceptionHandler);

                var stateMachineRef = new StateMachineRefHeader() 
                {
                    ComponentCode = componentCodeSent,
                    StateMachineCode = stateMachineCodeSent,
                    StateMachineId = stateMachineIdSent,
                    StateCode = stateCodeSent,
                    MessageType = messageTypeSent,
                    ErrorMessage = errorMessageSent
                };

                var basicProperties = new BasicProperties() 
                {
                    Headers = RabbitMqHeaderConverter.CreateHeaderFromStateMachineRefHeader(stateMachineRef, IncomingEventType.Transition, eventCode)
                };

                var stream = new MemoryStream();
                GetSerializer(Serialization).Serialize(stream, TestMessage);
                var message = stream.ToArray();
                consumer.HandleBasicDeliver(consumer.ConsumerTag, ulong.MaxValue, false, ExchangeName, PublicRoutingKey, basicProperties, message);

                var messageReceived = messageReceivedEvent.WaitOne(receptionTimeout);

                Check.That(messageReceived).IsTrue();
                Check.That(receivedMessageType).IsEqualTo(messageTypeSent);
                Check.That(componentCodeReceived).IsEqualTo(componentCodeSent);
                Check.That(stateMachineCodeReceived).IsEqualTo(stateMachineCodeSent);
                Check.That(stateMachineIdReceived).IsEqualTo(stateMachineIdSent);
                Check.That(stateCodeReceived).IsEqualTo(stateCodeSent);
                Check.That(errorMessageReceived).IsEqualTo(errorMessageSent);

                subscriber.Unsubscribe(StateMachineA, messageReceptionHandler);
            }
        }

        [TestCase(7, 7)]
        [TestCase(7, 2)]
        [TestCase(7, 3)]
        [TestCase(7, 10)]
        [TestCase(7, 0)]
        [TestCase(7, -1)]
        [TestCase(6, 6)]
        public void SnapshotTest(int instancesCount, int chunkSize)
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
            int componentCode = 1;
            int stateMachineCode = 2;
            int eventCode = 1;

            using (var publisher = new RabbitMqPublisher(ComponentNameA, configuration, connection, GetSerializer(SnapshotSerialization), PrivateCommincationIdentifier))
            using (var snapshotReceivedEvent = new AutoResetEvent(false))
            {
                List<MessageEventArgs> snapshotInstances = null;
                Task.Run(async () =>
                {
                    snapshotInstances = await publisher.GetSnapshotAsync(StateMachineA, chunkSize);
                }).GetAwaiter().OnCompleted(() =>
                {
                    snapshotReceivedEvent.Set();
                });

                var stateMachineRef = new StateMachineRefHeader 
                {
                    ComponentCode = componentCode,
                    StateMachineCode = stateMachineCode,
                    PrivateTopic = PublicRoutingKey
                };
                
                var instances = new List<StateMachineInstance>();

                for (var i = 0; i < instancesCount; i++)
                {
                    instances.Add(new StateMachineInstance() {
                        ComponentCode = componentCode,
                        StateMachineCode = stateMachineCode,
                        PublicMember = TestMessage
                    });
                }

                var chunks = SerializeSnapshotInstancesInChunks(instances, chunkSize);
                var basicProperties = new BasicProperties {
                    Headers = RabbitMqHeaderConverter.CreateHeaderFromStateMachineRefHeader(stateMachineRef, IncomingEventType.Snapshot, eventCode)
                };
                
                foreach (var chunk in chunks)
                {
                    Thread.Sleep(100);
                    consumer.HandleBasicDeliver(consumer.ConsumerTag, ulong.MaxValue, false, ExchangeName, snapshotReplyTopic, basicProperties, chunk);
                }

                var messageReceived = snapshotReceivedEvent.WaitOne(receptionTimeout);

                Check.That(messageReceived).IsTrue();
                Check.That(snapshotInstances.Count).IsEqualTo(instancesCount);
                Check.That(snapshotInstances.FirstOrDefault()?.MessageReceived).IsInstanceOf<string>();
                Check.That((string)(snapshotInstances.FirstOrDefault()?.MessageReceived)).IsEqualTo(TestMessage);
                snapshotReceivedEvent.Dispose();
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

        private List<byte[]> SerializeSnapshotInstancesInChunks(List<StateMachineInstance> instances, int? chunkSize = 1)
        {
            
            var chunks = new List<byte[]>();
            var size = 0;
            if (chunkSize.HasValue && chunkSize.Value <= 0)
            {
                size = instances.Count;
            }
            else
            {
                size = chunkSize.Value;
            }

            var chunksCount = 1;
            if (instances.Count > size)
            {
                chunksCount = instances.Count / size + 1;
            }

            for (var i = 0; i < chunksCount; i++)
            {
                var chunkInstances = new List<SnapshotItem>();

                for (var j = i* size; j < i* size + size && j < instances.Count; j++)
                {
                    chunkInstances.Add(new SnapshotItem()
                    {
                        ComponentCode = instances[j].ComponentCode,
                        StateMachineCode = instances[j].StateMachineCode,
                        StateMachineId = instances[j].StateMachineId,
                        StateCode = instances[j].StateCode,
                        PublicMember = instances[j].PublicMember
                    });
                    
                }

                var snapshotResponseChunk = new SnapshotResponseChunk 
                {
                    ChunkCount = chunksCount,
                    ChunkId = i,
                    KnownRuntimeIds = new List<string> { "localhost" },
                    Response = new SnapshotResponse 
                    {
                        Items = chunkInstances
                    },
                    RuntimeId = "localhost"
                };

                var serializedMessage = JsonConvert.SerializeObject(snapshotResponseChunk);
                var messageStream = new MemoryStream();
                GetSerializer(SnapshotSerialization).Serialize(messageStream, serializedMessage);
                messageStream.Flush();
                messageStream.Seek(0, SeekOrigin.Begin);

                chunks.Add(messageStream.ToArray());
            }

            return chunks;
        }
    }
}
