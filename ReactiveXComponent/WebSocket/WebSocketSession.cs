using System;
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

        public WebSocketSession(WebSocketEndpoint endpoint, int timeout, IXCConfiguration xcConfiguration, string privateCommunicationIdentifier)
        {
            _webSocketClient = new WebSocketClient();
            _webSocketClient.Init(endpoint, timeout);
            _webSocketClient.Open();
            _webSocketClient.ConnectionClosed += WebSocketClientOnConnectionClosed;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        private void WebSocketClientOnConnectionClosed(object sender, EventArgs eventArgs)
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }

        public bool IsOpen => _webSocketClient.IsOpen;

        public event EventHandler SessionClosed;

        public IXCPublisher CreatePublisher(string component)
        {
            throw new NotImplementedException();
        }

        public IXCSubscriber CreateSubscriber(string component)
        {
            return new WebSocketSubscriber(component, _webSocketClient, _xcConfiguration, _privateCommunicationIdentifier);
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
