using System;
using System.Collections.Generic;
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

        public WebSocketSession(WebSocketEndpoint endpoint, int timeout, IXCConfiguration xcConfiguration, string privateCommunicationIdentifier)
        {
            _webSocketClient = new WebSocketClient();
            _webSocketClient.Init(endpoint, timeout);
            _webSocketClient.Open();
            _webSocketClient.ConnectionClosed += WebSocketClientOnConnectionClosed;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _webSocketXCApiManager = new WebSocketXCApiManager(_webSocketClient);
        }

        private void WebSocketClientOnConnectionClosed(object sender, EventArgs eventArgs)
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }

        public bool IsOpen => _webSocketClient.IsOpen;

        public event EventHandler SessionClosed;

        public IXCPublisher CreatePublisher(string component)
        {
            return new WebSocketPublisher(component, _webSocketClient, _xcConfiguration, _privateCommunicationIdentifier);
        }

        public IXCSubscriber CreateSubscriber(string component)
        {
            return new WebSocketSubscriber(component, _webSocketClient, _xcConfiguration, _privateCommunicationIdentifier);
        }

        public List<string> GetXCApiList(int timeout = 10000)
        {
            return _webSocketXCApiManager.GetXCApiList(timeout);
        }

        public string GetXCApi(string apiFullName, int timeout = 10000)
        {
            return _webSocketXCApiManager.GetXCApi(apiFullName, timeout);
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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
