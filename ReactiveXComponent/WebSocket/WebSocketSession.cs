using System;
using System.Collections.Generic;
using System.IO;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketSession : IXCSession
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly WebSocketXCApiManager _webSocketXCApiManager;
        private readonly TimeSpan _retryInterval;
        private readonly int _maxRetries;

        public WebSocketSession(WebSocketEndpoint endpoint, int timeout, IXCConfiguration xcConfiguration, string privateCommunicationIdentifier, TimeSpan? retryInterval = null, int maxRetries = 5)
        {
            _retryInterval = (retryInterval != null) ? retryInterval.Value : TimeSpan.FromSeconds(5);
            _maxRetries = maxRetries;
            _webSocketClient = new WebSocketClient();
            _webSocketClient.Init(endpoint, TimeSpan.FromMilliseconds(timeout), _retryInterval, _maxRetries);
            _webSocketClient.ConnectionOpened += WebSocketClientOnConnectionOpened;
            _webSocketClient.ConnectionClosed += WebSocketClientOnConnectionClosed;
            _webSocketClient.ConnectionError += WebSocketClientOnConnectionError;
            _webSocketClient.Open();
            
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _webSocketXCApiManager = new WebSocketXCApiManager(_webSocketClient);
        }

        private void WebSocketClientOnConnectionOpened(object sender, EventArgs eventArgs)
        {
            SessionOpened?.Invoke(this, EventArgs.Empty);
        }

        private void WebSocketClientOnConnectionClosed(object sender, EventArgs eventArgs)
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }

        private void WebSocketClientOnConnectionError(object sender, ErrorEventArgs errorEventArgs)
        {
            ConnectionError?.Invoke(this, errorEventArgs);
        }

        public bool IsOpen => _webSocketClient.IsOpen;

        public event EventHandler SessionOpened;

        public event EventHandler SessionClosed;

        public event EventHandler<System.IO.ErrorEventArgs> ConnectionError;

        public IXCPublisher CreatePublisher(string component)
        {
            return new WebSocketPublisher(component, _webSocketClient, _xcConfiguration, _privateCommunicationIdentifier);
        }

        public IXCSubscriber CreateSubscriber(string component)
        {
            return new WebSocketSubscriber(component, _webSocketClient, _xcConfiguration, _privateCommunicationIdentifier);
        }

        public List<string> GetXCApiList(string requestId = null, TimeSpan ? timeout = null)
        {
            var delay = timeout ?? TimeSpan.FromSeconds(10);
            return _webSocketXCApiManager.GetXCApiList(requestId,delay);
        }

        public string GetXCApi(string apiFullName, string requestId = null, TimeSpan? timeout = null)
        {
            var delay = timeout ?? TimeSpan.FromSeconds(10);
            return _webSocketXCApiManager.GetXCApi(apiFullName, requestId, delay);
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _webSocketClient.ConnectionOpened -= WebSocketClientOnConnectionOpened;
                    _webSocketClient.ConnectionClosed -= WebSocketClientOnConnectionClosed;
                    _webSocketClient.Dispose();
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

        ~WebSocketSession()
        {
            Dispose(false);
        }

        #endregion
    }
}
