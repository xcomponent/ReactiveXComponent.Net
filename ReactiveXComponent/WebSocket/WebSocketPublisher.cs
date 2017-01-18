using System;
using System.Collections.Generic;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketPublisher : IXCPublisher
    {
        private readonly string _component;
        private readonly IWebSocketClient _webSocketClient;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly WebSocketSnapshotManager _webSocketSnapshotManager;

        public WebSocketPublisher(string component, IWebSocketClient webSocketClient, IXCConfiguration xcConfiguration, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _webSocketClient = webSocketClient;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _webSocketSnapshotManager = new WebSocketSnapshotManager(component, webSocketClient, xcConfiguration, privateCommunicationIdentifier);
        }

        #region IXCPublisher implementation

        public void SendEvent(string stateMachine, object message, Visibility visibility = Visibility.Public)
        {
            if (!_webSocketClient.IsOpen) return;
            
            var inputHeader = CreateWebSocketHeader(stateMachine, message, visibility);
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var topic = _xcConfiguration.GetPublisherTopic(_component, stateMachine);
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                    WebSocketCommand.Input,
                    inputHeader,
                    message,
                    componentCode.ToString(),
                    topic);

            _webSocketClient.Send(webSocketRequest);
        }

        public void SendEvent(StateMachineRefHeader stateMachineRefHeader, object message, Visibility visibility = Visibility.Public)
        {
            if (!_webSocketClient.IsOpen) return;

            var inputHeader = CreateWebSocketHeaderFromStateMachineRef(stateMachineRefHeader, message, visibility);
            var componentCode = _xcConfiguration.GetComponentCode(_component);
            var topic = _xcConfiguration.GetPublisherTopic(componentCode, stateMachineRefHeader.StateMachineCode);
            var webSocketRequest = WebSocketMessageHelper.SerializeRequest(
                    WebSocketCommand.Input,
                    inputHeader,
                    message,
                    componentCode.ToString(),
                    topic);

            _webSocketClient.Send(webSocketRequest);
        }

        public List<MessageEventArgs> GetSnapshot(string stateMachine, int timeout = 10000)
        {
            return _webSocketSnapshotManager.GetSnapshot(stateMachine, timeout);
        }

        public void GetSnapshotAsync(string stateMachine, Action<List<MessageEventArgs>> onSnapshotReceived)
        {
            _webSocketSnapshotManager.GetSnapshotAsync(stateMachine, onSnapshotReceived);
        }

        #endregion

        private WebSocketEngineHeader CreateWebSocketHeader(string stateMachine, object message, Visibility visibility = Visibility.Public)
        {
            var messageType = message?.GetType();
            var webSocketEngineHeader = new WebSocketEngineHeader
            {
                ComponentCode = new Option<long>(_xcConfiguration.GetComponentCode(_component)),
                StateMachineCode = new Option<long>(_xcConfiguration.GetStateMachineCode(_component, stateMachine)),
                EventCode = _xcConfiguration.GetPublisherEventCode(messageType?.ToString()),
                MessageType = new Option<string>(messageType?.ToString()),
                PublishTopic = visibility == Visibility.Private ? new Option<string>(_privateCommunicationIdentifier) : null
            };

            return webSocketEngineHeader;
        }

        private WebSocketEngineHeader CreateWebSocketHeaderFromStateMachineRef(StateMachineRefHeader smRefHeader, object message, Visibility visibility)
        {
            var messageType = message?.GetType();
            var webSocketEngineHeader = new WebSocketEngineHeader
            {
                AgentId = new Option<int>(smRefHeader.AgentId),
                StateMachineId = new Option<long>(smRefHeader.StateMachineId),
                ComponentCode = new Option<long>(smRefHeader.ComponentCode),
                StateMachineCode = new Option<long>(smRefHeader.StateMachineCode),
                StateCode = new Option<int>(smRefHeader.StateCode),
                EventCode = _xcConfiguration.GetPublisherEventCode(messageType?.ToString()),
                MessageType = new Option<string>(messageType?.ToString()),
                PublishTopic = visibility == Visibility.Private ? new Option<string>(_privateCommunicationIdentifier) : null
            };

            return webSocketEngineHeader;
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
                    _webSocketSnapshotManager.Dispose();
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

        ~WebSocketPublisher()
        {
            Dispose(false);
        }

        #endregion

    }
}
