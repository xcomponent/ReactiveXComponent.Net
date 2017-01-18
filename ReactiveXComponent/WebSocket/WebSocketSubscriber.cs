using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketSubscriber : IXCSubscriber
    {
        private readonly string _component;
        private readonly IWebSocketClient _webSocketClient;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        private readonly ConcurrentDictionary<SubscriptionKey, EventHandler<WebSocketSharp.MessageEventArgs>> _subscriptionsDico;
        private readonly ConcurrentDictionary<StreamSubscriptionKey, IDisposable> _streamSubscriptionsDico;

        private event EventHandler<MessageEventArgs> MessageReceived;

        public WebSocketSubscriber(string component, IWebSocketClient webSocketClient, IXCConfiguration xcConfiguration, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _webSocketClient = webSocketClient;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;

            _subscriptionsDico = new ConcurrentDictionary<SubscriptionKey, EventHandler<WebSocketSharp.MessageEventArgs>>();
            _streamSubscriptionsDico = new ConcurrentDictionary<StreamSubscriptionKey, IDisposable>();

            StateMachineUpdatesStream = Observable.FromEvent<EventHandler<MessageEventArgs>, MessageEventArgs>(
                handler => (sender, e) => handler(e),
                h => MessageReceived += h,
                h => MessageReceived -= h);

            SubscribeToPrivateTopic();
        }

        private void SubscribeToPrivateTopic()
        {
            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                var webSocketTopic = WebSocketTopic.Private(_privateCommunicationIdentifier);
                var webSocketSubscription = new WebSocketSubscription(webSocketTopic);
                var subscriptionHeader = new WebSocketEngineHeader();
                var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                    WebSocketCommand.Subscribe,
                    subscriptionHeader,
                    webSocketSubscription);

                _webSocketClient.Send(webSocketRequest);
            }
        }

        public IObservable<MessageEventArgs> StateMachineUpdatesStream { get; }

        public void Subscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            if (stateMachineListener == null)
            {
                return;
            }

            var isPrivateSubscription = !string.IsNullOrEmpty(_privateCommunicationIdentifier);
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var routingKey = isPrivateSubscription ? _privateCommunicationIdentifier : _xcConfiguration.GetSubscriberTopic(_component, stateMachine);

            var subscriptionKey = new SubscriptionKey(componentCode, stateMachineCode, routingKey);
            var streamSusbscriptionKey = new StreamSubscriptionKey(subscriptionKey, stateMachineListener);

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                // Init private subscription..
                InitSubscription(stateMachine, true);
            }

            var streamHandler = new Action<MessageEventArgs>(args =>
            {
                if (args.StateMachineRefHeader.StateMachineCode == stateMachineCode)
                {
                    stateMachineListener(args);
                }
            });

            var streamSubscription = StateMachineUpdatesStream.Subscribe(streamHandler);
            _streamSubscriptionsDico.AddOrUpdate(streamSusbscriptionKey, key => streamSubscription,
                (oldKey, oldValue) => streamSubscription);

            // Init public subscription..
            InitSubscription(stateMachine);
        }

        private void InitSubscription(string stateMachine, bool isPrivate = false)
        {
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var routingKey = isPrivate ? _privateCommunicationIdentifier : _xcConfiguration.GetSubscriberTopic(_component, stateMachine);

            var subscriptionKey = new SubscriptionKey(componentCode, stateMachineCode, routingKey);

            if (!_subscriptionsDico.ContainsKey(subscriptionKey))
            {
                var webSocketTopic = isPrivate ? WebSocketTopic.Private(routingKey) : WebSocketTopic.Public(routingKey);
                var webSocketSubscription = new WebSocketSubscription(webSocketTopic);
                var subscriptionHeader = new WebSocketEngineHeader();
                var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                    WebSocketCommand.Subscribe,
                    subscriptionHeader, 
                    webSocketSubscription);

                var handler = new EventHandler<WebSocketSharp.MessageEventArgs>((sender, messageEventArgs) =>
                {
                    var handlerTopic = routingKey;
                    string rawRequest = messageEventArgs.Data;
                    var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(rawRequest);

                    if (webSocketMessage.Topic == handlerTopic)
                    {
                        var receivedPacket = WebSocketMessageHelper.DeserializePacket(webSocketMessage);

                        var stateMachineRefHeader = new StateMachineRefHeader() {
                            ComponentCode = GetOptionalValue(receivedPacket.Header.ComponentCode),
                            EventCode = receivedPacket.Header.EventCode,
                            MessageType = GetOptionalValue(receivedPacket.Header.MessageType),
                            StateMachineCode = GetOptionalValue(receivedPacket.Header.StateMachineCode),
                            StateCode = GetOptionalValue(receivedPacket.Header.StateCode),
                            StateMachineId = GetOptionalValue(receivedPacket.Header.StateMachineId),
                            PublishTopic = GetOptionalValue(receivedPacket.Header.PublishTopic)
                        };

                        if (stateMachineRefHeader.StateMachineCode == stateMachineCode)
                        {
                            var receivedObject = WebSocketMessageHelper.DeserializeString(receivedPacket.JsonMessage);

                            var message = new MessageEventArgs(stateMachineRefHeader, receivedObject);

                            MessageReceived?.Invoke(this, message);
                        }
                    }
                });

                _webSocketClient.MessageReceived += handler;

                if (!isPrivate)
                {
                    _webSocketClient.Send(webSocketRequest);
                }

                _subscriptionsDico.AddOrUpdate(subscriptionKey, (key) => handler, (oldKey, oldValue) => oldValue);
            }
        }

        public void Unsubscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener)
        {
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var hasPrivateSubscription = !string.IsNullOrEmpty(_privateCommunicationIdentifier);

            if (hasPrivateSubscription)
            {
                var privateRoutingKey = _privateCommunicationIdentifier;
                var privateSubscriptionKey = new SubscriptionKey(componentCode, stateMachineCode, privateRoutingKey);

                EventHandler<WebSocketSharp.MessageEventArgs> subscriptionHandler;
                if (_subscriptionsDico.TryRemove(privateSubscriptionKey, out subscriptionHandler))
                {
                    _webSocketClient.MessageReceived -= subscriptionHandler;
                }
            }

            var publicRoutingKey = _xcConfiguration.GetSubscriberTopic(_component, stateMachine);
            var publicSubscriptionKey = new SubscriptionKey(componentCode, stateMachineCode, publicRoutingKey);

            EventHandler<WebSocketSharp.MessageEventArgs> publicSubscriptionHandler;
            if (_subscriptionsDico.TryRemove(publicSubscriptionKey, out publicSubscriptionHandler))
            {
                _webSocketClient.MessageReceived -= publicSubscriptionHandler;
                SendUnsubscribeMessage(publicRoutingKey, false);
            }

            var streamSusbscriptionKey = new StreamSubscriptionKey(publicSubscriptionKey, stateMachineListener);
            IDisposable streamSubscription;

            if (_streamSubscriptionsDico.TryRemove(streamSusbscriptionKey, out streamSubscription))
            {
                streamSubscription.Dispose();
            }
        }

        private void SendUnsubscribeMessage(string topic, bool isPrivate)
        {
            var webSocketTopic = isPrivate ? WebSocketTopic.Private(topic) : WebSocketTopic.Public(topic);
            var webSocketSubscription = new WebSocketSubscription(webSocketTopic);
            var subscriptionHeader = new WebSocketEngineHeader();
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                WebSocketCommand.Unsubscribe,
                subscriptionHeader,
                webSocketSubscription);
            _webSocketClient.Send(webSocketRequest);
        }

        private void UnsubscribeAll()
        {
            foreach (var subscription in _subscriptionsDico)
            {
                var subscriptionKey = subscription.Key;
                var subscriptionHandler = subscription.Value;

                _webSocketClient.MessageReceived -= subscriptionHandler;

                var isPrivateSubscription = _privateCommunicationIdentifier == subscriptionKey.RoutingKey;
                if (!isPrivateSubscription)
                {
                    SendUnsubscribeMessage(subscriptionKey.RoutingKey, false);
                }
            }

            SendUnsubscribeMessage(_privateCommunicationIdentifier, true);

            _subscriptionsDico.Clear();

            foreach (var streamSubscription in _streamSubscriptionsDico)
            {
                var streamSubscriber = streamSubscription.Value;
                streamSubscriber.Dispose();
            }

            _streamSubscriptionsDico.Clear();
        }

        private T GetOptionalValue<T>(Option<T> optionalValue)
        {
            return optionalValue != null ? optionalValue.Fields[0] : default(T);
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    UnsubscribeAll();
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

        ~WebSocketSubscriber()
        {
            Dispose(false);
        }

        #endregion

    }
}
