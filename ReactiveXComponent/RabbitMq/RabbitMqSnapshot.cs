using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSnapshot
    {
        private readonly IConnection _connection;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly ISerializer _serializer;
        private readonly string _component;

        private readonly ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos> _subscribers;

        private IModel _snapshotChannel;
        private event EventHandler<MessageEventArgs> SnapshotReceived;
        private event EventHandler<string> ConnectionFailed;
        private IObservable<MessageEventArgs> _snapshotStream;

        public List<MessageEventArgs> StateMachineInstances;

        public RabbitMqSnapshot(IConnection connection, string component, IXCConfiguration configuration, ISerializer serializer, string privateCommunicationIdentifier)
        {
            _connection = connection;
            _component = component;
            _xcConfiguration = configuration;
            _serializer = serializer;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _subscribers = new ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos>();
            CreateSnapshotChannel(connection);
            InitObservableCollection();
            StateMachineInstances = new List<MessageEventArgs>();
        }

        private void CreateSnapshotChannel(IConnection connection)
        {
            if (connection == null || !connection.IsOpen) return;

            var exchangeName = _xcConfiguration.GetComponentCode(_component).ToString();

            _snapshotChannel = connection.CreateModel();
            _snapshotChannel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
        }

        private void InitObservableCollection()
        {
            _snapshotStream = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => SnapshotReceived += h,
                h => SnapshotReceived -= h);
        }

        public void GetSnapshot(string stateMachine, Action<MessageEventArgs> OnSnapshotReceived = null, int timeout = 0)
        {
            var guid = Guid.NewGuid();
            InitSnapshotSubscriber(stateMachine, guid.ToString());
            SendSnapshotRequest(stateMachine, guid, _privateCommunicationIdentifier);

            if (OnSnapshotReceived != null)
            {
                _snapshotStream.Subscribe(OnSnapshotReceived);
            }

            if (timeout == 0) return;
            var lockEvent = new AutoResetEvent(false);
            SnapshotReceived += (sender, args) =>
            {
                var stateMachineInstancesList =
                    JsonConvert.DeserializeObject<List<StateMachineInstance>>(args.MessageReceived.ToString());
                foreach (var element in stateMachineInstancesList)
                {
                    var stateMachineRefHeader = new StateMachineRefHeader()
                    {
                        AgentId = element.AgentId,
                        StateMachineId = element.StateMachineId,
                        ComponentCode = element.ComponentCode,
                        StateMachineCode = element.StateMachineCode,
                        StateCode = element.StateCode
                    };

                    var messageEventArgs = new MessageEventArgs(stateMachineRefHeader, element.PublicMember);
                    StateMachineInstances.Add(messageEventArgs);
                }
                lockEvent.Set();
            }; 
            if (!lockEvent.WaitOne(timeout))
            {
                throw new ReactiveXComponentException("Snapshot not received");
            }
        }

        private void SendSnapshotRequest(string stateMachine, Guid guid, string privateCommunicationIdentifier = null)
        {
            if (_xcConfiguration == null)
                return;

            var snapshotheader = new Header
            {
                MessageType = "XComponent.Common.Processing.SnapshotIncomingEvent",
                IncomingEventType = (int)IncomingEventType.Snapshot
            };

            var routingKey = _xcConfiguration.GetSnapshotTopic(_component);
            var prop = _snapshotChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.ConvertHeader(snapshotheader);

            var snapshotMessage = new SnapshotMessage()
            {
                StateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine),
                ComponentCode = _xcConfiguration.GetComponentCode(_component),
                 
                ReplyTopic = new ReplyTopic()
                {
                    Case = "Some",
                    Fields = new[] {guid.ToString()}
                },
                PrivateTopic = new PrivateTopic()
                {
                    Case = "Some",
                    Fields = new[,] 
                    {
                        {
                            !string.IsNullOrEmpty(privateCommunicationIdentifier)
                            ? privateCommunicationIdentifier
                            : string.Empty
                        }
                    }
                }
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

        private void InitSnapshotSubscriber(string stateMachine, string replyTopic)
        {
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

                var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(_component), _xcConfiguration.GetStateMachineCode(_component, stateMachine));
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

        private void ReceiveMessage(SubscriberKey subscriberKey)
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
                    DispatchMessage(e);
                };
            }
            catch (EndOfStreamException ex)
            {
                ConnectionFailed?.Invoke(this, "Snapshot has been interrupted : " + ex.Message);
            }
        }

        private void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs)
        {
            var obj = _serializer.Deserialize(new MemoryStream(basicAckEventArgs.Body));
            
            var stateMachineRefHeader =
                RabbitMqHeaderConverter.ConvertStateMachineRefHeader(basicAckEventArgs.BasicProperties.Headers);
            var replyTopic = (stateMachineRefHeader?.MessageType?.Split('.').Last()).Contains("Snapshot")
                ? basicAckEventArgs.RoutingKey
                : string.Empty;
            dynamic unzipedObj = JsonConvert.DeserializeObject(obj.ToString());
            byte[] compressed = Convert.FromBase64String(unzipedObj.Items.Value);
            var message = string.Empty;
            using (var msi = new MemoryStream(compressed))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                message = Encoding.UTF8.GetString(mso.ToArray());
            }
            var msgEventArgs = new MessageEventArgs(stateMachineRefHeader, message);

            OnSnapshotReceived(msgEventArgs);
        }

        private void OnSnapshotReceived(MessageEventArgs e)
        {
            SnapshotReceived?.Invoke(this, e);
        }
    }
}
