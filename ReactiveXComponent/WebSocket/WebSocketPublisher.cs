using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            SendEvent(stateMachine, message, message?.GetType().ToString(), visibility);
        }

        public void SendEvent(string stateMachine, object message, string messageType, Visibility visibility = Visibility.Public)
        {
            if (!_webSocketClient.IsOpen) return;

            var inputHeader = CreateWebSocketHeader(stateMachine, messageType, visibility);
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

        public List<MessageEventArgs> GetSnapshot(string stateMachine, int? chunkSize, int timeout = 10000)
        {
            return _webSocketSnapshotManager.GetSnapshot(stateMachine, chunkSize, timeout);
        }

        public Task<List<MessageEventArgs>> GetSnapshotAsync(string stateMachine, int? chunkSize, int timeout = 10000)
        {
            return _webSocketSnapshotManager.GetSnapshotAsync(stateMachine, chunkSize, timeout);
        }

        #endregion

        private WebSocketEngineHeader CreateWebSocketHeader(string stateMachine, string messageType, Visibility visibility = Visibility.Public)
        {
            var webSocketEngineHeader = new WebSocketEngineHeader
            {
                ComponentCode = _xcConfiguration.GetComponentCode(_component),
                StateMachineCode = _xcConfiguration.GetStateMachineCode(_component, stateMachine),
                EventCode = _xcConfiguration.GetPublisherEventCode(messageType),
                MessageType = messageType,
                PublishTopic = visibility == Visibility.Private && !string.IsNullOrEmpty(_privateCommunicationIdentifier)? _privateCommunicationIdentifier : null
            };

            return webSocketEngineHeader;
        }

        private WebSocketEngineHeader CreateWebSocketHeaderFromStateMachineRef(StateMachineRefHeader smRefHeader, object message, Visibility visibility)
        {
            var messageType = message?.GetType();
            var webSocketEngineHeader = new WebSocketEngineHeader
            {
                StateMachineId = smRefHeader.StateMachineId,
                ComponentCode = smRefHeader.ComponentCode,
                StateMachineCode = smRefHeader.StateMachineCode,
                StateCode = smRefHeader.StateCode,
                EventCode = _xcConfiguration.GetPublisherEventCode(messageType?.ToString()),
                MessageType = messageType?.ToString(),
                PublishTopic = visibility == Visibility.Private && !string.IsNullOrEmpty(_privateCommunicationIdentifier)? _privateCommunicationIdentifier : null
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
