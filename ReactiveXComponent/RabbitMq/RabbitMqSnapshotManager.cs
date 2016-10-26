﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    public class RabbitMqSnapshotManager : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly ISerializer _serializer;
        private readonly string _component;

        private readonly ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos> _subscribers;

        private IModel _snapshotChannel;
        private event EventHandler<List<MessageEventArgs>> SnapshotReceived;
        private event EventHandler<string> ConnectionFailed;
        private IObservable<List<MessageEventArgs>> _snapshotStream;

        public RabbitMqSnapshotManager(IConnection connection, string component, IXCConfiguration configuration, ISerializer serializer, string privateCommunicationIdentifier = null)
        {
            _connection = connection;
            _component = component;
            _xcConfiguration = configuration;
            _serializer = serializer;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _subscribers = new ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos>();
            CreateSnapshotChannel(connection);
            InitObservableCollection();
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
            _snapshotStream = Observable.FromEvent<EventHandler<List<MessageEventArgs>>, List<MessageEventArgs>>(
                handler => (sender, e) => handler(e),
                h => SnapshotReceived += h,
                h => SnapshotReceived -= h);
        }

        public List<MessageEventArgs> GetSnapshot(string stateMachine, int timeout = 10000)
        {
            var guid = Guid.NewGuid();
            SubscribeSnapshot(stateMachine, guid.ToString());
            SendSnapshotRequest(stateMachine, guid, _privateCommunicationIdentifier);

            List<MessageEventArgs> result = null;
            var lockEvent = new AutoResetEvent(false);
            var handler = new EventHandler<List<MessageEventArgs>>((sender, args) =>
            {
                result = new List<MessageEventArgs>(args);
                lockEvent.Set();
            });
            SnapshotReceived += handler; 

            lockEvent.WaitOne(timeout);

            SnapshotReceived -= handler;
            UnsubscribeSnapshot(stateMachine);

            return result;
        }

        public void GetSnapshotAsync(string stateMachine, Action<List<MessageEventArgs>> onSnapshotReceived)
        {
            var guid = Guid.NewGuid();
            SubscribeSnapshot(stateMachine, guid.ToString());
            SendSnapshotRequest(stateMachine, guid, _privateCommunicationIdentifier);

            if (onSnapshotReceived != null)
            {
                _snapshotStream.Subscribe(onSnapshotReceived);
            }
            UnsubscribeSnapshot(stateMachine);
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

        private void SubscribeSnapshot(string stateMachine, string replyTopic)
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
            dynamic unzipedObj = JsonConvert.DeserializeObject(obj.ToString());
            byte[] compressed = Convert.FromBase64String(unzipedObj.Items.Value);
            string message;
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
            var stateMachineInstances = new List<MessageEventArgs>();
            var messageReceived =
                JsonConvert.DeserializeObject<List<StateMachineInstance>>(e.MessageReceived.ToString());
            foreach (var element in messageReceived)
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
                stateMachineInstances.Add(messageEventArgs);
            }
            SnapshotReceived?.Invoke(this, stateMachineInstances);
        }

        private void UnsubscribeSnapshot(string stateMachineCode)
        {
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(_component), _xcConfiguration.GetStateMachineCode(_component, stateMachineCode));

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
