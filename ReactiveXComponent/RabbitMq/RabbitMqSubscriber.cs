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

        private event EventHandler<MessageEventArgs> MessageReceived;
        private event EventHandler<string> ConnectionFailed;

        public IObservable<MessageEventArgs> StateMachineUpdatesStream { get; private set; }

        public RabbitMqSubscriber(string component, IXCConfiguration xcConfiguration, IConnection connection, ISerializer serializer, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _subscribers = new ConcurrentDictionary<SubscriberKey, RabbitMqSubscriberInfos>();
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
            if (stateMachineListener == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                InitSubscriber(stateMachine, _privateCommunicationIdentifier);
            }

            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);

            StateMachineUpdatesStream.Subscribe(args =>
            {
                if (args.StateMachineRefHeader.StateMachineCode == stateMachineCode)
                {
                    stateMachineListener(args);
                }
            });

            InitSubscriber(stateMachine);
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
                var subscriberKey = new SubscriberKey(componentCode, stateMachineCode, routingKey);
                if (!_subscribers.ContainsKey(subscriberKey))
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

                    _subscribers.AddOrUpdate(subscriberKey, rabbitMqSubscriberInfos, (key, oldValue) => rabbitMqSubscriberInfos);
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

        private void DeleteSubscription(SubscriberKey subscriberkey)
        {
            RabbitMqSubscriberInfos rabbitMqSubscriberInfos;

            if (_subscribers.TryRemove(subscriberkey, out rabbitMqSubscriberInfos))
            {
                rabbitMqSubscriberInfos.Channel.ModelShutdown -= ChannelOnModelShutdown;
                rabbitMqSubscriberInfos.Channel.BasicCancel(rabbitMqSubscriberInfos.Subscriber.ConsumerTag);
                rabbitMqSubscriberInfos.Channel.Dispose();
            }
        }

        private void OnMessageReceived(MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public void Unsubscribe(string stateMachine)
        {
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var publicRoutingKey = _xcConfiguration.GetSubscriberTopic(_component, stateMachine);
            var publicSubscriberKey = new SubscriberKey(componentCode, stateMachineCode, publicRoutingKey);

            DeleteSubscription(publicSubscriberKey);

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                var privateSubscriberKey = new SubscriberKey(componentCode, stateMachineCode, _privateCommunicationIdentifier);
                DeleteSubscription(privateSubscriberKey);
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
                    foreach (var subscriberInfo in _subscribers.Values)
                    {
                        subscriberInfo.Channel.ModelShutdown -= ChannelOnModelShutdown;
                        subscriberInfo.Channel.BasicCancel(subscriberInfo.Subscriber.ConsumerTag);
                        subscriberInfo.Channel.Dispose();
                    }

                    _subscribers.Clear();

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
