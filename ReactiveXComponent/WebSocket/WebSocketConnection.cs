using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using WebSocketSharp;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketConnection : IXCConnection
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        public WebSocketConnection(IXCConfiguration xcConfiguration, WebSocketEndpoint endpoint, int timeout, string privateCommunicationIdentifier = null)
        {
            _webSocketClient = new WebSocketClient();
            _webSocketClient.Init(endpoint, timeout);
            _webSocketClient.Open();
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public IXCSession CreateSession()
        {
            return new WebSocketSession(_webSocketClient, _xcConfiguration, _privateCommunicationIdentifier);
        }


        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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

        ~WebSocketConnection()
        {
            Dispose(false);
        }

        #endregion

    }
}
