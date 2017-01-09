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

        private readonly ConcurrentDictionary<SubscriptionKey, RabbitMqSubscriberInfos> _subscribersDico;
        private readonly ConcurrentDictionary<StreamSubscriptionKey, IDisposable> _streamSubscriptionsDico;

        private event EventHandler<MessageEventArgs> MessageReceived;
        private event EventHandler<string> ConnectionFailed;

        public RabbitMqSubscriber(string component, IXCConfiguration xcConfiguration, IConnection connection, ISerializer serializer, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _subscribersDico = new ConcurrentDictionary<SubscriptionKey, RabbitMqSubscriberInfos>();
            _streamSubscriptionsDico = new ConcurrentDictionary<StreamSubscriptionKey, IDisposable>();
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializer = serializer;
            InitObservableCollection();
        }

        #region IXCSubscriber implementation

        public IObservable<MessageEventArgs> StateMachineUpdatesStream { get; private set; }

        public void Subscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            if (stateMachineListener == null)
            {
                return;
            }

            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var routingKey = string.IsNullOrEmpty(_privateCommunicationIdentifier) ? _xcConfiguration.GetSubscriberTopic(_component, stateMachine) : _privateCommunicationIdentifier;
            var subscriptionKey = new SubscriptionKey(componentCode, stateMachineCode, routingKey);
            var streamSusbcriptionKey = new StreamSubscriptionKey(subscriptionKey, stateMachineListener);

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                InitSubscriber(stateMachine, _privateCommunicationIdentifier);
            }

            var handler = new Action<MessageEventArgs>(args => 
                {
                    if (args.StateMachineRefHeader.StateMachineCode == stateMachineCode)
                    {
                        stateMachineListener(args);
                    }
                });

            var subscription = StateMachineUpdatesStream.Subscribe(handler);

            _streamSubscriptionsDico.AddOrUpdate(streamSusbcriptionKey, subscription, (key, oldSubscription) => subscription);

            InitSubscriber(stateMachine);
        }

        public void Unsubscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var publicRoutingKey = _xcConfiguration.GetSubscriberTopic(_component, stateMachine);
            var publicSubscriberKey = new SubscriptionKey(componentCode, stateMachineCode, publicRoutingKey);

            DeleteSubscription(publicSubscriberKey, stateMachineListener);

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                var privateSubscriberKey = new SubscriptionKey(componentCode, stateMachineCode, _privateCommunicationIdentifier);
                DeleteSubscription(privateSubscriberKey, stateMachineListener);
            }
        }

        #endregion

        private void InitObservableCollection()
        {
            StateMachineUpdatesStream = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => MessageReceived += h,
                h => MessageReceived -= h);
        }

        private void OnMessageReceived(MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private void InitSubscriber(string stateMachine, string privateCommunicationIdentifier = null)
        {
            if (_xcConfiguration == null)
            {
                return;
            }

            var exchangeName = _xcConfiguration.GetComponentCode(_component).ToString();
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var routingKey = string.IsNullOrEmpty(privateCommunicationIdentifier) ? _xcConfiguration.GetSubscriberTopic(_component, stateMachine) : privateCommunicationIdentifier;

            try
            {
                var subscriptionKey = new SubscriptionKey(componentCode, stateMachineCode, routingKey);
                if (!_subscribersDico.ContainsKey(subscriptionKey))
                {
                    IModel channel;
                    EventingBasicConsumer subscriber;
                    CreateExchangeChannel(exchangeName, routingKey, out channel, out subscriber);

                    if (channel == null || subscriber == null)
                    {
                        return;
                    }

                    var rabbitMqSubscriberInfos = new RabbitMqSubscriberInfos
                    {
                        Channel = channel,
                        Subscriber = subscriber
                    };

                    rabbitMqSubscriberInfos.Subscriber.Received += (o, basicAckEventArgs) => 
                    {
                        rabbitMqSubscriberInfos.Channel?.BasicAck(basicAckEventArgs.DeliveryTag, false);

                        var stateMachineRefHeader = RabbitMqHeaderConverter.ConvertStateMachineRefHeader(basicAckEventArgs.BasicProperties.Headers);

                        if (stateMachineRefHeader.StateMachineCode == stateMachineCode)
                        {
                            var obj = _serializer.Deserialize(new MemoryStream(basicAckEventArgs.Body));

                            var msgEventArgs = new MessageEventArgs(stateMachineRefHeader, obj);

                            OnMessageReceived(msgEventArgs);
                        }
                    };

                    _subscribersDico.AddOrUpdate(subscriptionKey, rabbitMqSubscriberInfos, (key, oldValue) => rabbitMqSubscriberInfos);
                }  
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

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ConnectionFailed?.Invoke(this, shutdownEventArgs.ReplyText);
        }

        private void DeleteSubscriber(SubscriptionKey subscriptionKey)
        {
            var canBeRemoved = true;

            foreach (var element in _streamSubscriptionsDico.Keys)
            {
                if (subscriptionKey.Equals(element.SubscriberKey))
                {
                    canBeRemoved = false;
                }
            }

            if (canBeRemoved)
            {
                RabbitMqSubscriberInfos rabbitMqSubscriberInfos;
                if (_subscribersDico.TryRemove(subscriptionKey, out rabbitMqSubscriberInfos))
                {
                    rabbitMqSubscriberInfos.Channel.ModelShutdown -= ChannelOnModelShutdown;
                    rabbitMqSubscriberInfos.Channel.BasicCancel(rabbitMqSubscriberInfos.Subscriber.ConsumerTag);
                    rabbitMqSubscriberInfos.Channel.Dispose();
                }
            }
        }

        private void DeleteSubscription(SubscriptionKey subscriptionKey, Action<MessageEventArgs> stateMachineListener)
        {
            var streamSubscriptionKey = new StreamSubscriptionKey(subscriptionKey, stateMachineListener);

            IDisposable subscription;

            if (_streamSubscriptionsDico.TryRemove(streamSubscriptionKey, out subscription))
            {
                subscription.Dispose();
                DeleteSubscriber(subscriptionKey);
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
                    foreach (var subscriberInfo in _subscribersDico.Values)
                    {
                        subscriberInfo.Channel.ModelShutdown -= ChannelOnModelShutdown;
                        subscriberInfo.Channel.BasicCancel(subscriberInfo.Subscriber.ConsumerTag);
                        subscriberInfo.Channel.Dispose();
                    }

                    _subscribersDico.Clear();

                    foreach (var subscription in _streamSubscriptionsDico.Values)
                    {
                        subscription.Dispose();    
                    }

                    _streamSubscriptionsDico.Clear();
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
