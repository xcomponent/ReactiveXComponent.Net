using System;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveXComponent.RabbitMQ;
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

        private readonly Dictionary<SubscriberKey, RabbitMqSubscriberInfos> _subscribers;
        private readonly Dictionary<SubscriberKey, List<Action<MessageEventArgs>>> _listenerToUpdateBySubscriberKeyRepo;

        public event EventHandler<MessageEventArgs> MessageReceived;
        private IObservable<MessageEventArgs> _xcObservable;

        private readonly SerializerFactory _serializerFactory;

        public RabbitMqSubscriber(IXCConfiguration xcConfiguration, IConnection connection, SerializerFactory serializerFactory, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _subscribers = new Dictionary<SubscriberKey, RabbitMqSubscriberInfos>();
            _listenerToUpdateBySubscriberKeyRepo = new Dictionary<SubscriberKey, List<Action<MessageEventArgs>>>();
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializerFactory = serializerFactory;
        }

        public void Subscribe(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            if (stateMachineListener == null) return;
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
            AddToSubscribersRepository(subscriberKey);
            AddToListenerToUpdateBySubscriberKeyRepository(subscriberKey, stateMachineListener);
           
            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                InitPrivateSubscriber(component, stateMachine);
            }
            else
            {
                InitObservableCollection(component, stateMachine);
                _xcObservable.Subscribe(stateMachineListener);
                InitSubscriber(component, stateMachine);
            }
        }

        private void InitPrivateSubscriber(string component, string stateMachine)
        {
            InitObservableCollection(component, stateMachine);
            _xcObservable.Subscribe(args =>
            {
                var subscribreKey = new SubscriberKey(args.SatetMachineRef.ComponentCode, args.SatetMachineRef.StateMachineCode);
                List<Action<MessageEventArgs>> listenerToUpdateList;
                if (!_listenerToUpdateBySubscriberKeyRepo.TryGetValue(subscribreKey, out listenerToUpdateList)) return;
                foreach (var listener in listenerToUpdateList)
                {
                    listener(args);
                }
            });
            InitSubscriber(component, stateMachine);
        }

        private void InitObservableCollection(string component, string stateMachine)
        {
            _xcObservable = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => MessageReceived += h,
                h => MessageReceived -= h)
                .Where(k => k.SatetMachineRef.ComponentCode == _xcConfiguration.GetComponentCode(component) &&
                            k.SatetMachineRef.StateMachineCode == _xcConfiguration.GetStateMachineCode(component, stateMachine));
        }

        public IObservable<MessageEventArgs> GetStateMachineUpdates()
        {
            return _xcObservable;
        }

        private void InitSubscriber(string component, string stateMachine)
        {
            var exchangeName = _xcConfiguration?.GetComponentCode(component).ToString();
            var routingKey = _xcConfiguration?.GetSubscriberTopic(component, stateMachine);
            try
            {
                if (_connection == null || !_connection.IsOpen) return;
                var channel = _connection.CreateModel();

                channel.ModelShutdown += ChannelOnModelShutdown;
                channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                var queueName = channel.QueueDeclare().QueueName;
                var subscriber = new EventingBasicConsumer(channel);
                channel.BasicConsume(queueName, false, subscriber);
                channel.QueueBind(queueName, exchangeName, routingKey, null);

                var rabbitMqSubscriberInfos = new RabbitMqSubscriberInfos
                {
                    Channel = channel,
                    Subscriber = subscriber
                };
                var subscriberKey = new SubscriberKey(Convert.ToInt32(exchangeName),
                    _xcConfiguration.GetStateMachineCode(component, stateMachine));

                if (!_subscribers.ContainsKey(subscriberKey))
                {
                    _subscribers.Add(subscriberKey, rabbitMqSubscriberInfos);
                }
                ReceiveMessage(rabbitMqSubscriberInfos);
            }
            catch (OperationInterruptedException e)
            {
                throw new Exception("Init Subscriber failed", e);
            }
        }

        private void ReceiveMessage(RabbitMqSubscriberInfos rabbitMqSubscriberInfos)
        {
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

        private void AddToSubscribersRepository(SubscriberKey subscriberKey)
        {
            if (!_subscribers.ContainsKey(subscriberKey))
            {
                _subscribers.Add(subscriberKey, new RabbitMqSubscriberInfos());
            }
        }

        private void AddToListenerToUpdateBySubscriberKeyRepository(SubscriberKey subscriberKey, Action<MessageEventArgs> stateMachineListener)
        {
            if (!_listenerToUpdateBySubscriberKeyRepo.ContainsKey(subscriberKey))
            {
                _listenerToUpdateBySubscriberKeyRepo.Add(subscriberKey, new List<Action<MessageEventArgs>>());
            }   
            _listenerToUpdateBySubscriberKeyRepo[subscriberKey].Add(stateMachineListener);
        }

        public event EventHandler<string> ConnectionFailed;

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ConnectionFailed?.Invoke(this, shutdownEventArgs.ReplyText);
        }

        private void Unsubscribe(SubscriberKey consumerkey)
        {
            if (!_subscribers.ContainsKey(consumerkey)) return;
            _subscribers[consumerkey].Channel.ModelShutdown -= ChannelOnModelShutdown;

            _subscribers[consumerkey].Subscriber.OnCancel();
            _subscribers[consumerkey].Channel.Close();

            _subscribers.Remove(consumerkey);
        }

        private void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs)
        {
            var serializer = _serializerFactory.CreateSerializer();
            var obj = serializer.Deserialize(new MemoryStream(basicAckEventArgs.Body));
            var msgEventArgs = new MessageEventArgs(
                RabbitMqHeaderConverter.ConvertHeader(basicAckEventArgs.BasicProperties.Headers),
                obj);
            OnMessageReceived(msgEventArgs);
        }

        private void OnMessageReceived(MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public void DeleteStateMachineListener(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
            if (_listenerToUpdateBySubscriberKeyRepo[subscriberKey] == null || !_listenerToUpdateBySubscriberKeyRepo[subscriberKey].Contains(stateMachineListener)) return;
            Unsubscribe(subscriberKey);
            _listenerToUpdateBySubscriberKeyRepo[subscriberKey].Remove(stateMachineListener);
        }

        private void Close()
        {
            foreach (var subscriberkey in _listenerToUpdateBySubscriberKeyRepo.Keys)
            {
                Unsubscribe(subscriberkey);
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
