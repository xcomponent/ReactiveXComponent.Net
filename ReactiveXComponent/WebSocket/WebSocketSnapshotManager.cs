using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Newtonsoft.Json;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketSnapshotManager : IDisposable
    {
        private readonly string _component;
        private readonly string _privateCommunicationIdentifier;
        private readonly IWebSocketClient _webSocketClient;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly ConcurrentDictionary<SubscriptionKey, EventHandler<WebSocketMessageEventArgs>> _subscriptions;
        private readonly ConcurrentDictionary<SubscriptionKey, IDisposable> _streamSubscriptionsDico;
        private SerializationType _serializationType;

        private event EventHandler<List<MessageEventArgs>> SnapshotReceived;

        private readonly IObservable<List<MessageEventArgs>> _snapshotStream;

        public WebSocketSnapshotManager(string component, IWebSocketClient webSocketClient, IXCConfiguration xcConfiguration, string privateCommunicationIdentifier)
        {
            _component = component;
            _webSocketClient = webSocketClient;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _subscriptions = new ConcurrentDictionary<SubscriptionKey, EventHandler<WebSocketMessageEventArgs>>();
            _streamSubscriptionsDico = new ConcurrentDictionary<SubscriptionKey, IDisposable>();
            InitSerializationType();

            _snapshotStream = Observable.FromEvent<EventHandler<List<MessageEventArgs>>, List<MessageEventArgs>>(
                handler => (sender, e) => handler(e),
                h => SnapshotReceived += h,
                h => SnapshotReceived -= h);
        }
        
        public List<MessageEventArgs> GetSnapshot(string stateMachine, int timeout = 10000)
        {
            var replyTopic = Guid.NewGuid().ToString();

            List <MessageEventArgs> result = null;
            var lockEvent = new AutoResetEvent(false);
            var observer = Observer.Create<List<MessageEventArgs>>(message =>
            {
                result = new List<MessageEventArgs>(message);
                lockEvent.Set();
            });

            EventHandler<WebSocketMessageEventArgs> subscriptionHandler;
            CreateSnapshotReplyHandler(replyTopic, out subscriptionHandler);
            _webSocketClient.MessageReceived += subscriptionHandler;

            using (_snapshotStream.Subscribe(observer))
            {
                SendWebSocketSnapshotSubscriptionResquest(replyTopic);
                SendWebSocketSnapshotRequest(stateMachine, replyTopic);
                lockEvent.WaitOne(timeout);
            }

            _webSocketClient.MessageReceived -= subscriptionHandler;
            SendUnsubscribeSnapshot(replyTopic);

            return result;
        }

        public void GetSnapshotAsync(string stateMachine, Action<List<MessageEventArgs>> onSnapshotReceived)
        {
            var replyTopic = Guid.NewGuid().ToString();
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var subscriptionKey = new SubscriptionKey(componentCode, stateMachineCode, replyTopic);
            EventHandler<WebSocketMessageEventArgs> subscriptionHandler;

            CreateSnapshotReplyHandler(replyTopic, out subscriptionHandler);
            _webSocketClient.MessageReceived += subscriptionHandler;
            var snapshotSubscription = _snapshotStream.Subscribe(onSnapshotReceived);

            SendWebSocketSnapshotSubscriptionResquest(replyTopic);
            SendWebSocketSnapshotRequest(stateMachine, replyTopic);

            _subscriptions.AddOrUpdate(subscriptionKey, key => subscriptionHandler, (oldKey, oldValue) => oldValue);
            _streamSubscriptionsDico.AddOrUpdate(subscriptionKey, key => snapshotSubscription, (oldKey, oldValue) => oldValue);
        }

        private void SendWebSocketSnapshotRequest(string stateMachine, string replyTopic)
        {
            if (!_webSocketClient.IsOpen) return;

            var inputHeader = new WebSocketEngineHeader();
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var topic = _xcConfiguration.GetSnapshotTopic(_component);
            var stateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine);
            var snapshotMessage = new WebSocketSnapshotMessage(stateMachineCode, componentCode, replyTopic, _privateCommunicationIdentifier);
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                    WebSocketCommand.Snapshot,
                    inputHeader,
                    snapshotMessage,
                    componentCode.ToString(),
                    topic);

            _webSocketClient.Send(webSocketRequest);
        }

        private void CreateSnapshotReplyHandler(string replyTopic, out EventHandler<WebSocketMessageEventArgs> subscriptionHandler)
        {
            subscriptionHandler = (sender, args) =>
            {
                var handlerTopic = replyTopic;
                string rawRequest = args.Data;
                var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(rawRequest);

                if (webSocketMessage.Topic == handlerTopic)
                {
                    var receivedPacket = WebSocketMessageHelper.DeserializeSnapshot(webSocketMessage.Json);
                    var stateMachineInstances = new List<MessageEventArgs>();
                    var snapshotReceived = JsonConvert.DeserializeObject<List<StateMachineInstance>>(receivedPacket);

                    foreach (var element in snapshotReceived)
                    {
                        var stateMachineRefHeader = new StateMachineRefHeader()
                        {
                            AgentId = element.AgentId,
                            StateMachineId = element.StateMachineId,
                            ComponentCode = element.ComponentCode,
                            StateMachineCode = element.StateMachineCode,
                            StateCode = element.StateCode
                        };

                        var messageEventArgs = new MessageEventArgs(stateMachineRefHeader, element.PublicMember, _serializationType);
                        stateMachineInstances.Add(messageEventArgs);
                    }

                    SnapshotReceived?.Invoke(this, stateMachineInstances);
                }
            };
        }


        private void SendWebSocketSnapshotSubscriptionResquest(string replyTopic)
        {
            var webSocketTopic = WebSocketTopic.Snapshot(replyTopic);
            var webSocketSubscription = new WebSocketSubscription(webSocketTopic);
            var subscriptionHeader = new WebSocketEngineHeader();
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                WebSocketCommand.Subscribe,
                subscriptionHeader,
                webSocketSubscription);

            _webSocketClient.Send(webSocketRequest);
        }

        private void SendUnsubscribeSnapshot(string replyTopic)
        {
            var webSocketTopic = WebSocketTopic.Snapshot(replyTopic);
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
            foreach (var subscription in _subscriptions)
            {
                var subscriptionKey = subscription.Key;
                var subscriptionHandler = subscription.Value;

                _webSocketClient.MessageReceived -= subscriptionHandler;
                SendUnsubscribeSnapshot(subscriptionKey.RoutingKey);
            }

            _subscriptions.Clear();

            foreach (var streamSubscription in _streamSubscriptionsDico)
            {
                var streamSubscriber = streamSubscription.Value;
                streamSubscriber.Dispose();
            }

            _streamSubscriptionsDico.Clear();
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

        ~WebSocketSnapshotManager()
        {
            Dispose(false);
        }

        #endregion
    }
}
