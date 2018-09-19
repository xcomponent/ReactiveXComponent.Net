using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSnapshotManager : IDisposable
    {
        private const string SnapshotMessageType = "XComponent.Common.Processing.SnapshotIncomingEvent";
        private readonly IConnection _connection;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly ISerializer _serializer;
        private readonly string _component;
        private SerializationType _serializationType;

        private readonly ConcurrentDictionary<SubscriptionKey, RabbitMqSubscriberInfos> _subscribers;

        private IModel _snapshotChannel;
        private event EventHandler<ChunkedSnapshotEvent> SnapshotReceived;
        private event EventHandler<string> ConnectionFailed;

        public RabbitMqSnapshotManager(IConnection connection, string component, IXCConfiguration configuration, ISerializer serializer, string privateCommunicationIdentifier = null)
        {
            _connection = connection;
            _component = component;
            _xcConfiguration = configuration;
            _serializer = serializer;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _subscribers = new ConcurrentDictionary<SubscriptionKey, RabbitMqSubscriberInfos>();
            InitSerializationType();
            CreateSnapshotChannel(connection);
        }

        private void CreateSnapshotChannel(IConnection connection)
        {
            if (connection == null || !connection.IsOpen) return;

            var exchangeName = _xcConfiguration.GetComponentCode(_component).ToString();

            _snapshotChannel = connection.CreateModel();
            _snapshotChannel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
        }

        private static SnapshotResponse AggregateChunks(ConcurrentBag<SnapshotResponseChunk> snapshotChunks)
        {
            if (snapshotChunks.Count == 0)
            {
                return null;
            }

            var aggregatedResult = new SnapshotResponse();

            SnapshotResponseChunk chunk;
            while (snapshotChunks.TryTake(out chunk))
            {
                aggregatedResult.Items.AddRange(chunk.Response.Items);
            }
            return aggregatedResult;
        }

        public List<MessageEventArgs> GetSnapshot(string stateMachine, int? chunkSize = null, int timeout = 10000)
        {
            var guid = Guid.NewGuid();
            var requestId = guid.ToString();
            List<MessageEventArgs> result = null;

            var receivedSnapshotChunksInitialized = new AutoResetEvent(false);
            var receivedSnapshotChunks = new ConcurrentDictionary<string, ChunkCountdown>();
            var snapshotChunks = new ConcurrentBag<SnapshotResponseChunk>();
            List<object> invalidChunks = new List<object>();

            try
            {
                EventHandler<ChunkedSnapshotEvent> snapshotListenerOnMessageReceived = (sender, args) =>
                {
                    if (args.RequestId != requestId)
                    {
                        return;
                    }

                    var chunk = args.SnapshotResponseChunk;
                    if (chunk == null)
                    {
                        invalidChunks.Add(args);
                    }
                    else
                    {
                        snapshotChunks.Add(chunk);
                        foreach (var runtimeId in chunk.KnownRuntimeIds)
                        {
                            receivedSnapshotChunks.TryAdd(runtimeId, new ChunkCountdown());
                        }
                        receivedSnapshotChunksInitialized.Set();

                        ChunkCountdown chunkCountdown;
                        if (receivedSnapshotChunks.TryGetValue(chunk.RuntimeId, out chunkCountdown))
                        {
                            chunkCountdown.SetValueIfNotInitialized(chunk.ChunkCount);
                            chunkCountdown.Decrement();
                        }
                    }
                };

                SnapshotReceived += snapshotListenerOnMessageReceived;

                SubscribeSnapshot(stateMachine, requestId);
                SendSnapshotRequest(stateMachine, requestId, chunkSize, _privateCommunicationIdentifier);

                if (receivedSnapshotChunksInitialized.WaitOne(timeout))
                {
                    var completionEvents =
                        receivedSnapshotChunks.Values.Select(ccd => ccd.CompletionResetEvent).ToArray();
                    if (WaitHandle.WaitAll(completionEvents, timeout))
                    {
                        SnapshotReceived -= snapshotListenerOnMessageReceived;
                        return AggregateChunks(snapshotChunks).Items.Select(item =>
                            new MessageEventArgs(new StateMachineRefHeader()
                                {
                                    StateMachineId = item.StateMachineId,
                                    StateMachineCode = item.StateMachineCode,
                                    ComponentCode = item.ComponentCode,
                                    StateCode = item.StateCode,
                                    WorkerId = item.WorkerId,
                                },
                                item.PublicMember,
                                _serializationType)).ToList();
                    }
                }

                SnapshotReceived -= snapshotListenerOnMessageReceived;

                UnsubscribeSnapshot(stateMachine);

                if (invalidChunks.Count > 0)
                {
                    throw new ReactiveXComponentException($"A number of {invalidChunks.Count} chunks are not of the expected type {nameof(SnapshotResponseChunk)}");
                }

                if (!receivedSnapshotChunks.IsEmpty)
                {
                    var errorMessage = new StringBuilder("The snapshot was incomplete: ");
                    foreach (var remainingSnapshotChunk in receivedSnapshotChunks)
                    {
                        var remainingChunkCount = remainingSnapshotChunk.Value.Countdown == Int32.MinValue
                            ? "all"
                            : remainingSnapshotChunk.Value.Countdown.ToString();
                        errorMessage.Append(
                            $" runtime {remainingSnapshotChunk.Key} missing {remainingChunkCount} chunk(s);");
                    }

                    throw new ReactiveXComponentException(errorMessage.ToString());
                }
            }
            catch (Exception e)
            {
                throw new ReactiveXComponentException("Error encoutered while requesting snapshot: " + e.Message, e);
            }
            finally
            {
                foreach (var chunkCountdown in receivedSnapshotChunks.Values)
                {
                    chunkCountdown.Dispose();
                }

                receivedSnapshotChunksInitialized.Dispose();
            }

            return result;
        }

        public Task<List<MessageEventArgs>> GetSnapshotAsync(string stateMachine, int? chunkSize = null, int timeout = 10000)
        {
            return Task.Run(() => GetSnapshot(stateMachine, chunkSize, timeout));
        }

        private void SendSnapshotRequest(string stateMachine, string replyTopic, int? chunkSize, string privateCommunicationIdentifier = null)
        {
            if (_xcConfiguration == null)
                return;

            var snapshotheader = new Header
            {
                MessageType = SnapshotMessageType,
                IncomingEventType = (int)IncomingEventType.Snapshot,
                StateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine),
                ComponentCode = _xcConfiguration.GetComponentCode(_component)
            };

            var routingKey = _xcConfiguration.GetSnapshotTopic(_component);
            var prop = _snapshotChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.ConvertHeader(snapshotheader);

            var snapshotMessage = new SnapshotMessage()
            {
                Timeout = TimeSpan.FromSeconds(10),
                ReplyTopic = replyTopic,
                CallerPrivateTopic = !string.IsNullOrEmpty(privateCommunicationIdentifier)
                            ? new List<string>{ privateCommunicationIdentifier}
                            : null,
                ChunkSize = chunkSize
            };

            Send(snapshotMessage, routingKey, prop);
        }

        private void Send(object message, string routingKey, IBasicProperties properties)
        {
            if (message == null)
                throw new ReactiveXComponentException("Message is null");

            byte[] messageBytes;
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(stream, message);
                messageBytes = stream.ToArray();
            }

            try
            {
                var exchangeName = _xcConfiguration.GetComponentCode(_component).ToString();
                _snapshotChannel.BasicPublish(exchangeName, routingKey, properties, messageBytes);
            }
            catch (Exception exception)
            {
                throw new ReactiveXComponentException("Snapshot request failed: " + exception.Message, exception);
            }
        }

        private void SubscribeSnapshot(string stateMachine, string replyTopic)
        {
            if (_serializer is BinarySerializer)
            {
                throw new ReactiveXComponentException("Binary serialization is not supported");
            }

            if (_xcConfiguration == null)
                return;

            var exchangeName = _xcConfiguration.GetComponentCode(_component).ToString();
            var routingKey = replyTopic;

            try
            {
                IModel channel;
                EventingBasicConsumer subscriber;
                CreateExchangeChannel(exchangeName, routingKey, out channel, out subscriber);
                if (channel == null || subscriber == null) return;
                var rabbitMqSubscriberInfos = new RabbitMqSubscriberInfos
                {
                    Channel = channel,
                    Subscriber = subscriber
                };

                var subscriberKey = new SubscriptionKey(_xcConfiguration.GetComponentCode(_component), _xcConfiguration.GetStateMachineCode(_component, stateMachine), routingKey);
                _subscribers.AddOrUpdate(subscriberKey, rabbitMqSubscriberInfos, (key, oldValue) => rabbitMqSubscriberInfos);

                ReceiveMessage(subscriberKey);
            }
            catch (OperationInterruptedException e)
            {
                throw new ReactiveXComponentException("Failed to init Snapshot Subscriber: " + e.Message, e);
            }
        }

        private void CreateExchangeChannel(string exchangeName, string routingKey, out IModel channel, out EventingBasicConsumer subscriber)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                channel = null;
                subscriber = null;
                return;
            }

            channel = _connection.CreateModel();
            channel.ModelShutdown += ChannelOnModelShutdown;
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
            var queueName = channel.QueueDeclare().QueueName;
            subscriber = new EventingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, subscriber);
            channel.QueueBind(queueName, exchangeName, routingKey, null);
        }

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ConnectionFailed?.Invoke(this, shutdownEventArgs.ReplyText);
        }

        private void ReceiveMessage(SubscriptionKey subscriberKey)
        {
            RabbitMqSubscriberInfos rabbitMqSubscriberInfos;
            _subscribers.TryGetValue(subscriberKey, out rabbitMqSubscriberInfos);
            if (rabbitMqSubscriberInfos == null)
                return;

            try
            {
                rabbitMqSubscriberInfos.Subscriber.Received += (o, e) =>
                {
                    rabbitMqSubscriberInfos.Channel?.BasicAck(e.DeliveryTag, false);
                    DispatchMessage(e, subscriberKey.RoutingKey);
                };
            }
            catch (EndOfStreamException ex)
            {
                ConnectionFailed?.Invoke(this, "Snapshot has been interrupted : " + ex.Message);
            }
        }

        private void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs, string requestId)
        {
            var obj = _serializer.Deserialize(new MemoryStream(basicAckEventArgs.Body));
            
            var snapshotResponseChunk = JsonConvert.DeserializeObject<SnapshotResponseChunk>(obj.ToString());
            
            var chunkedSnapshotEvent = new ChunkedSnapshotEvent 
            {
                RequestId = requestId,
                SnapshotResponseChunk = snapshotResponseChunk
            };

            SnapshotReceived?.Invoke(this, chunkedSnapshotEvent);
        }

        private void UnsubscribeSnapshot(string stateMachine)
        {
            var routingKey = string.IsNullOrEmpty(_privateCommunicationIdentifier) ? _xcConfiguration.GetSubscriberTopic(_component, stateMachine) : _privateCommunicationIdentifier;
            var subscriberKey = new SubscriptionKey(_xcConfiguration.GetComponentCode(_component), _xcConfiguration.GetStateMachineCode(_component, stateMachine), routingKey);

            RabbitMqSubscriberInfos rabbitMqSubscriberInfos;
            _subscribers.TryRemove(subscriberKey, out rabbitMqSubscriberInfos);
            if (rabbitMqSubscriberInfos == null)
            {
                return;
            }

            rabbitMqSubscriberInfos.Channel.ModelShutdown -= ChannelOnModelShutdown;
            rabbitMqSubscriberInfos.Subscriber.OnCancel();
            rabbitMqSubscriberInfos.Channel.Close();
        }

        private void InitSerializationType()
        {
            var serialization = _xcConfiguration.GetSerializationType();

            switch (serialization)
            {
                case XCApiTags.Binary:
                    _serializationType = SerializationType.Binary;
                    break;
                case XCApiTags.Json:
                    _serializationType = SerializationType.Json;
                    break;
                case XCApiTags.Bson:
                    _serializationType = SerializationType.Bson;
                    break;
                default:
                    throw new XCSerializationException("Serialization type not supported");
            }
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // clear managed resources
                    _snapshotChannel?.Dispose();
                }
                // clear unmanaged resources
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RabbitMqSnapshotManager()
        {
            Dispose(false);
        }

        #endregion
    }
}
