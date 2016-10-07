using System;
using System.Collections.Concurrent;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reactive.Linq;
using RabbitMQ.Client.Exceptions;
using ReactiveXComponent.Common;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSubscriber : IXCSubscriber
    {
        private readonly IConnection _connection;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly ISerializer _serializer;
        private readonly string _component;

        private readonly ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos> _subscribers;
        private readonly ConcurrentDictionary<SubscriberKey, ThreadSafeList<Action<MessageEventArgs>>> _listenerBySubscriberKeyRepo;

        private event EventHandler<MessageEventArgs> MessageReceived;
        private event EventHandler<string> ConnectionFailed;

        public IObservable<MessageEventArgs> StateMachineUpdatesStream { get; private set; }

        public RabbitMqSubscriber(string component, IXCConfiguration xcConfiguration, IConnection connection, ISerializer serializer, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _subscribers = new ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos>();
            _listenerBySubscriberKeyRepo = new ConcurrentDictionary<SubscriberKey, ThreadSafeList<Action<MessageEventArgs>>>();
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializer = serializer;
            InitObservableCollection();
        }

        private void InitObservableCollection()
        {
            StateMachineUpdatesStream = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => MessageReceived += h,
                h => MessageReceived -= h);
        }

        public void Subscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            if (stateMachineListener == null) return;
            
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(_component), _xcConfiguration.GetStateMachineCode(_component, stateMachine));
            AddListenerToRepository(subscriberKey, stateMachineListener);

            StateMachineUpdatesStream = StateMachineUpdatesStream
                .Where(k => k.StateMachineRefHeader.ComponentCode == _xcConfiguration.GetComponentCode(_component) &&
                            k.StateMachineRefHeader.StateMachineCode == _xcConfiguration.GetStateMachineCode(_component, stateMachine));

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                InitPrivateSubscriber(stateMachine);
            }
            else
            {
                StateMachineUpdatesStream.Subscribe(stateMachineListener);
                InitSubscriber(stateMachine);
            }
        }

        private void InitPrivateSubscriber(string stateMachine)
        {
            StateMachineUpdatesStream.Subscribe(args =>
            {
                var subscribreKey = new SubscriberKey(args.StateMachineRefHeader.ComponentCode, args.StateMachineRefHeader.StateMachineCode);

                ThreadSafeList<Action<MessageEventArgs>> listenerToUpdateList;
                if (!_listenerBySubscriberKeyRepo.TryGetValue(subscribreKey, out listenerToUpdateList))
                {
                    return;
                }

                foreach (var listener in listenerToUpdateList)
                {
                    listener(args);
                }
            });

            InitSubscriber(stateMachine, _privateCommunicationIdentifier);
        }

        private void InitSubscriber(string stateMachine, string privateCommunicationIdentifier = null)
        {
            if (_xcConfiguration == null)
            {
                return;
            }

            var exchangeName = _xcConfiguration.GetComponentCode(_component).ToString();
            var routingKey = string.IsNullOrEmpty(privateCommunicationIdentifier) ? _xcConfiguration.GetSubscriberTopic(_component, stateMachine) : privateCommunicationIdentifier;

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
                throw new ReactiveXComponentException("Failed to init Subscriber: " + e.Message, e);
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

        private void ReceiveMessage(SubscriberKey subscriberKey)
        {
            RabbitMqSubscriberInfos rabbitMqSubscriberInfos;
            _subscribers.TryGetValue(subscriberKey, out rabbitMqSubscriberInfos);
            if (rabbitMqSubscriberInfos == null)
            {
                return;
            }

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
                ConnectionFailed?.Invoke(this, "Subscriber has been interrupted : " + ex.Message);
            }    
        }

        private void AddListenerToRepository(SubscriberKey subscriberKey, Action<MessageEventArgs> stateMachineListener)
        {
            _listenerBySubscriberKeyRepo.AddOrUpdate(
                subscriberKey, 
                key => new ThreadSafeList<Action<MessageEventArgs>> { stateMachineListener },
                (key, oldValue) =>
                {
                    oldValue.Add(stateMachineListener);
                    return oldValue;
                });
        }

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ConnectionFailed?.Invoke(this, shutdownEventArgs.ReplyText);
        }

        private void DeleteSubscription(SubscriberKey subscriberkey)
        {
            RabbitMqSubscriberInfos rabbitMqSubscriberInfos;
            _subscribers.TryRemove(subscriberkey, out rabbitMqSubscriberInfos);
            if (rabbitMqSubscriberInfos == null)
            {
                return;
            }

            rabbitMqSubscriberInfos.Channel.ModelShutdown -= ChannelOnModelShutdown;
            rabbitMqSubscriberInfos.Subscriber.OnCancel();
            rabbitMqSubscriberInfos.Channel.Close();
        }

        private void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs)
        {
            var obj = _serializer.Deserialize(new MemoryStream(basicAckEventArgs.Body));

            var msgEventArgs = new MessageEventArgs(
                                    RabbitMqHeaderConverter.ConvertStateMachineRefHeader(basicAckEventArgs.BasicProperties.Headers),
                                    obj);

            OnMessageReceived(msgEventArgs);
        }

        private void OnMessageReceived(MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public void Unsubscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(_component), _xcConfiguration.GetStateMachineCode(_component, stateMachine));

            DeleteSubscription(subscriberKey);

            ThreadSafeList<Action<MessageEventArgs>> listStateMachineListener;

            if (_listenerBySubscriberKeyRepo.TryGetValue(subscriberKey, out listStateMachineListener))
            {
                listStateMachineListener.TryRemove(stateMachineListener);
            }
        }

        private void Close()
        {
            foreach (var subscriberkey in _listenerBySubscriberKeyRepo.Keys)
            {
                DeleteSubscription(subscriberkey);
            }

            _listenerBySubscriberKeyRepo.Clear();
            _connection.Close();
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
                    Close();
                    _connection?.Dispose();
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

        ~RabbitMqSubscriber()
        {
            Dispose(false);
        }

        #endregion

    }
}
