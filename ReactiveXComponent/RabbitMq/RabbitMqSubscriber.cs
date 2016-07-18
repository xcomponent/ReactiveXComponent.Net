using System;
using System.Collections.Concurrent;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Reactive.Linq;
using ReactiveXComponent.Common;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSubscriber : IXCSubscriber
    {
        private bool _disposed;
        private readonly IConnection _connection;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        private readonly ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos> _subscribers;
        private readonly ConcurrentDictionary<SubscriberKey, ThreadSafeList<Action<MessageEventArgs>>> _listenerToUpdateBySubscriberKeyRepo;

        public event EventHandler<MessageEventArgs> MessageReceived;
        private IObservable<MessageEventArgs> _xcObservable;

        private readonly ISerializer _serializer;

        public RabbitMqSubscriber(IXCConfiguration xcConfiguration, IConnection connection, SerializerFactory serializerFactory, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _subscribers = new ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos>();
            _listenerToUpdateBySubscriberKeyRepo = new ConcurrentDictionary<SubscriberKey, ThreadSafeList<Action<MessageEventArgs>>>();
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializer = serializerFactory?.CreateSerializer();
            InitObservableCollection();
        }

        private void InitObservableCollection()
        {
            _xcObservable = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => MessageReceived += h,
                h => MessageReceived -= h);
        }

        public void Subscribe(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            if (stateMachineListener == null) return;
            
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
            AddToListenerToUpdateBySubscriberKeyRepository(subscriberKey, stateMachineListener);

            _xcObservable = _xcObservable
                .Where(k => k.StateMachineRef.ComponentCode == _xcConfiguration.GetComponentCode(component) &&
                            k.StateMachineRef.StateMachineCode == _xcConfiguration.GetStateMachineCode(component, stateMachine));

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                InitPrivateSubscriber(component, stateMachine);
            }
            else
            {
                _xcObservable.Subscribe(stateMachineListener);
                InitSubscriber(component, stateMachine);
            }
        }

        private void InitPrivateSubscriber(string component, string stateMachine)
        {
            _xcObservable.Subscribe(args =>
            {
                var subscribreKey = new SubscriberKey(args.StateMachineRef.ComponentCode,
                    args.StateMachineRef.StateMachineCode);
                ThreadSafeList<Action<MessageEventArgs>> listenerToUpdateList;
                if (!_listenerToUpdateBySubscriberKeyRepo.TryGetValue(subscribreKey, out listenerToUpdateList))
                    return;
                foreach (var listener in listenerToUpdateList)
                {
                    listener(args);
                }
            });
            InitSubscriber(component, stateMachine, _privateCommunicationIdentifier);
        }

        public IObservable<MessageEventArgs> GetStateMachineUpdates()
        {
            return _xcObservable;
        }

        private void InitSubscriber(string component, string stateMachine, string privateCommunicationIdentifier = null)
        {
            if (_xcConfiguration == null) return;
            var exchangeName = _xcConfiguration.GetComponentCode(component).ToString();
            var routingKey = string.IsNullOrEmpty(privateCommunicationIdentifier) ? _xcConfiguration.GetSubscriberTopic(component, stateMachine) : privateCommunicationIdentifier;
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

                var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
                _subscribers.AddOrUpdate(subscriberKey, rabbitMqSubscriberInfos, (key, oldValue) => rabbitMqSubscriberInfos);
                ReceiveMessage(subscriberKey);
            }
            catch (OperationInterruptedException e)
            {
                throw new Exception("Init Subscriber failed", e);
            }
        }

        private void CreateExchangeChannel(string exchangeName, string routingKey, out IModel channel,
            out EventingBasicConsumer subscriber)
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
            if (rabbitMqSubscriberInfos == null) return;
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

        private void AddToListenerToUpdateBySubscriberKeyRepository(SubscriberKey subscriberKey, Action<MessageEventArgs> stateMachineListener)
        {
            _listenerToUpdateBySubscriberKeyRepo.AddOrUpdate(subscriberKey, 
            key => new ThreadSafeList<Action<MessageEventArgs>> { stateMachineListener },
            (key, oldValue) =>
            {
                oldValue.Add(stateMachineListener);
                return oldValue;
            });
        }

        public event EventHandler<string> ConnectionFailed;

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ConnectionFailed?.Invoke(this, shutdownEventArgs.ReplyText);
        }

        private void DeleteSubscription(SubscriberKey subscriberkey)
        {
            RabbitMqSubscriberInfos rabbitMqSubscriberInfos;
            _subscribers.TryRemove(subscriberkey, out rabbitMqSubscriberInfos);
            if (rabbitMqSubscriberInfos == null) return;
            rabbitMqSubscriberInfos.Channel.ModelShutdown -= ChannelOnModelShutdown;
            rabbitMqSubscriberInfos.Subscriber.OnCancel();
            rabbitMqSubscriberInfos.Channel.Close();
        }

        private void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs)
        {
            var obj = _serializer.Deserialize(new MemoryStream(basicAckEventArgs.Body));
            var msgEventArgs = new MessageEventArgs(
                RabbitMqHeaderConverter.ConvertStateMachineRef(basicAckEventArgs.BasicProperties.Headers),
                obj);
            OnMessageReceived(msgEventArgs);
        }

        private void OnMessageReceived(MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public void Unsubscribe(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
            DeleteSubscription(subscriberKey);
            ThreadSafeList<Action<MessageEventArgs>> listStateMachineListener;
            _listenerToUpdateBySubscriberKeyRepo.TryGetValue(subscriberKey, out listStateMachineListener);
            if (listStateMachineListener == null || !listStateMachineListener.Contains(stateMachineListener)) return;
                listStateMachineListener.Remove(stateMachineListener);
        }

        private void Close()
        {
            foreach (var subscriberkey in _listenerToUpdateBySubscriberKeyRepo.Keys)
            {
                DeleteSubscription(subscriberkey);
            }
            _listenerToUpdateBySubscriberKeyRepo.Clear();
            _connection.Close();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Close();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
