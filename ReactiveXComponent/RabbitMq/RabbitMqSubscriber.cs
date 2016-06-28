using System;
using System.IO;
using RabbitMQ.Client;
using System.Threading;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveXComponent.RabbitMQ;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Configuration;
using System.Runtime.Serialization.Formatters.Binary;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSubscriber : IXCSubscriber
    {
        private bool _disposed;
        private readonly IConnection _connection;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        private readonly Dictionary<SubscriberKey, RabbitMqSubscriberInfos> _subscribers;
        private readonly Dictionary<SubscriberKey, List<Action<MessageEventArgs>>> _callbacksBySubscriberKey;

        public event EventHandler<MessageEventArgs> MessageReceived;
        private IObservable<MessageEventArgs> _xcObservable;

        public RabbitMqSubscriber(IXCConfiguration xcConfiguration, IConnection connection, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _connection = connection;
            _subscribers = new Dictionary<SubscriberKey, RabbitMqSubscriberInfos>();
            _callbacksBySubscriberKey = new Dictionary<SubscriberKey, List<Action<MessageEventArgs>>>();
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _xcObservable = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => MessageReceived += h,
                h => MessageReceived -= h);
        }

        public void AddCallback(string component, string stateMachine, Action<MessageEventArgs> callback)
        {
            if (callback == null) return;
            _xcObservable = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => MessageReceived += h,
                h => MessageReceived -= h)
                .Select(k => k)
                .Where(k => k.Header.ComponentCode == _xcConfiguration.GetComponentCode(component) && k.Header.StateMachineCode == _xcConfiguration.GetStateMachineCode(component, stateMachine));
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
            AddToSubscribersRepository(subscriberKey);
            AddToCallBacksBySubscriberRepository(subscriberKey, callback);
           
            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                InitPrivateSubscriber(component, stateMachine);
            }
            else
            {
                _xcObservable.Subscribe(callback);
                Subscribe(component, stateMachine);
            }
        }

        private void InitPrivateSubscriber(string component, string stateMachine)
        {
            _xcObservable.Subscribe(args =>
            {
                if (args.Header.StateMachineCode != _xcConfiguration.GetStateMachineCode(component, stateMachine) ||
                    args.Header.ComponentCode != _xcConfiguration.GetComponentCode(component)) return;
                var subscribreKey = new SubscriberKey(args.Header.ComponentCode, args.Header.StateMachineCode);
                List<Action<MessageEventArgs>> callBackList;
                if (!_callbacksBySubscriberKey.TryGetValue(subscribreKey, out callBackList)) return;
                foreach (var callBack in callBackList)
                {
                    callBack(args);
                }
            });
            Subscribe(component, stateMachine);
        }

        private void Subscribe(string component, string stateMachine)
        {
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
            RabbitMqSubscriberInfos consumerChannel;
            _subscribers.TryGetValue(subscriberKey, out consumerChannel);
            try
            {
                if (consumerChannel == null || consumerChannel.IsOpen) return;
                if (_connection == null || !_connection.IsOpen) return;
                var exchangeName = _xcConfiguration?.GetComponentCode(component).ToString();
                var routingKey = _xcConfiguration?.GetSubscriberTopic(component, stateMachine);
                var channel = _connection.CreateModel();
                channel.ModelShutdown += ChannelOnModelShutdown;
                channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);
                var queueName = channel.QueueDeclare().QueueName;
                var subscriber = new EventingBasicConsumer(channel);
                channel.BasicConsume(queueName, false, subscriber);
                channel.QueueBind(queueName, exchangeName, routingKey, null);

                var replyChannel = _connection.CreateModel();
                replyChannel.ModelShutdown += ChannelOnModelShutdown;
                        
                var thread = new Thread(Listen);
                thread.Start( new RabbitMqSubscriberInfos()
                {
                    Channel = channel,
                    ReplyChannel = replyChannel,
                    Subscriber = subscriber,
                    IsOpen = true
                });
                _subscribers[subscriberKey].Channel = channel;
                _subscribers[subscriberKey].ReplyChannel = replyChannel;
                _subscribers[subscriberKey].IsOpen = true;
                _subscribers[subscriberKey].Subscriber = subscriber;
            }
            catch (OperationInterruptedException e)
            {
                throw new Exception("Start consumer failure", e);
            }
        }

        private void AddToSubscribersRepository(SubscriberKey subscriberKey)
        {
            if (!_subscribers.ContainsKey(subscriberKey))
            {
                _subscribers.Add(subscriberKey, new RabbitMqSubscriberInfos());
            }
        }

        private void AddToCallBacksBySubscriberRepository(SubscriberKey subscriberKey, Action<MessageEventArgs> callback)
        {
            if (_callbacksBySubscriberKey.ContainsKey(subscriberKey))
            {
                _callbacksBySubscriberKey[subscriberKey].Add(callback);
            }
            else
            {
                _callbacksBySubscriberKey.Add(subscriberKey, new List<Action<MessageEventArgs>>());
                _callbacksBySubscriberKey[subscriberKey].Add(callback);
            }
        }

        public delegate void ConnectionFailure(object sender, StringEventArgs reason);

        public event ConnectionFailure ConnectionFailed;

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            var stringEventArgs = new StringEventArgs(shutdownEventArgs.ReplyText);
            ConnectionFailed?.Invoke(this, stringEventArgs);
        }

        private void Unsubscribe(SubscriberKey consumerkey)
        {
            if (!_subscribers.ContainsKey(consumerkey)) return;
            _subscribers[consumerkey].IsOpen = false;
            _subscribers[consumerkey].Channel.ModelShutdown -= ChannelOnModelShutdown;
            _subscribers[consumerkey].ReplyChannel.ModelShutdown -= ChannelOnModelShutdown;

            _subscribers[consumerkey].Subscriber.OnCancel();
            _subscribers[consumerkey].Channel.Close();

            _subscribers.Remove(consumerkey);
        }

        private void Listen(object args)
        {
            var subscriberInfos = args as RabbitMqSubscriberInfos;

            while (subscriberInfos != null && subscriberInfos.IsOpen)
            {
                try
                {
                    subscriberInfos.Subscriber.Received += (o, e) =>
                    {
                        subscriberInfos.Channel?.BasicAck(e.DeliveryTag, false);

                        DispatchMessage(e);
                    };
                }
                catch (EndOfStreamException ex)
                {
                    subscriberInfos.IsOpen = false;
                    var stringEventArgs = new StringEventArgs("Subscriber has been interrupted : " + ex.Message);
                    ConnectionFailed?.Invoke(this, stringEventArgs);
                }
            }
        }

        private void OnMessageReceived(MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs)
        {
            var binaryFormater = new BinaryFormatter();

            var obj = binaryFormater.Deserialize(new MemoryStream(basicAckEventArgs.Body));
            var msgEventArgs = new MessageEventArgs(
                RabbitMqHeaderConverter.ConvertHeader(basicAckEventArgs.BasicProperties.Headers),
                obj);
            OnMessageReceived(msgEventArgs);
        }

        public void RemoveCallback(string component, string stateMachine, Action<MessageEventArgs> callback)
        {
            var subscriberKey = new SubscriberKey(_xcConfiguration.GetComponentCode(component), _xcConfiguration.GetStateMachineCode(component, stateMachine));
            if (_callbacksBySubscriberKey[subscriberKey] == null || !_callbacksBySubscriberKey[subscriberKey].Contains(callback)) return;
            Unsubscribe(subscriberKey);
            _callbacksBySubscriberKey[subscriberKey].Remove(callback);
        }

        private void Close()
        {
            foreach (var subscriberkey in _callbacksBySubscriberKey.Keys)
            {
                Unsubscribe(subscriberkey);
            }
            _callbacksBySubscriberKey.Clear();
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
