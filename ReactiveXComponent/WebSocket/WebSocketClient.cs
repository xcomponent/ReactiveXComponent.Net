using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ReactiveXComponent.Common;
using WebSocketSharp;

namespace ReactiveXComponent.WebSocket
{
    internal class WebSocketClient : IWebSocketClient
    {
        private WebSocketSharp.WebSocket _webSocket;
        private readonly object _webSocketLock = new object();
        private AutoResetEvent _socketOpenEvent;
        private AutoResetEvent _socketCloseEvent;

        private WebSocketEndpoint _endpoint;
        private int _timeout;

        public event EventHandler<EventArgs> ConnectionOpened;
        public event EventHandler<EventArgs> ConnectionClosed;
        public event EventHandler<System.IO.ErrorEventArgs> ConnectionError;
        public event EventHandler<WebSocketMessageEventArgs> MessageReceived;

        private void OpenConnection()
        {
            _socketOpenEvent = new AutoResetEvent(false);
            _socketCloseEvent = new AutoResetEvent(false);
            var serverUri = GetServerUri();
            _webSocket = new WebSocketSharp.WebSocket(serverUri);

            _webSocket.Compression = CompressionMethod.Deflate;
            _webSocket.SslConfiguration.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;

            _webSocket.OnOpen += OnWebSocketOpened;
            _webSocket.OnClose += OnWebSocketClosed;
            _webSocket.OnError += OnWebSocketError;
            _webSocket.Connect();

            if (!_socketOpenEvent.WaitOne(_timeout))
            {
                throw new ReactiveXComponentException($"Could not connect to the web socket server {serverUri} after {_timeout} ms");
            }

            _webSocket.OnMessage += OnMessageReceived;
        }

        private void CloseConnection()
        {
            if (CanClose())
            {
                lock (_webSocketLock)
                {
                    if (CanClose())
                    {
                        _webSocket.Close();
                        _socketCloseEvent.WaitOne(_timeout);
                    }
                }
            }
        }

        private bool CanClose()
        {
            return _webSocket != null;
        }

        private string GetServerUri()
        {
            var uriBuilder = new UriBuilder(_endpoint.Host);

            var protocolRegex = new Regex(@"\w*://.*");

            var match = protocolRegex.Match(_endpoint.Host);
            if (match.Success)
            {
                uriBuilder.Port = int.Parse(_endpoint.Port);
            }
            else
            {
                uriBuilder.Port = int.Parse(_endpoint.Port);
                uriBuilder.Scheme = _endpoint.Type == WebSocketType.Secure ? "wss" : "ws";
            }

            return uriBuilder.ToString();
        }

        private void OnWebSocketOpened(object sender, EventArgs eventArgs)
        {
            _socketOpenEvent.Set();

            ConnectionOpened?.Invoke(this, EventArgs.Empty);
        }

        private void OnWebSocketClosed(object sender, EventArgs eventArgs)
        {
            _socketCloseEvent.Set();

            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }

        private void OnWebSocketError(object sender, ErrorEventArgs errorEventArgs)
        {
            ConnectionError?.Invoke(this, new System.IO.ErrorEventArgs(errorEventArgs.Exception));
        }

        private void OnMessageReceived(object sender, WebSocketSharp.MessageEventArgs messageEventArgs)
        {
            MessageReceived?.Invoke(sender, new WebSocketMessageEventArgs(messageEventArgs.Data, messageEventArgs.RawData));
        }

        public bool IsOpen { get { return _webSocket != null && _webSocket.ReadyState == WebSocketState.Open; } }

        public void Init(WebSocketEndpoint endpoint, int timeout)
        {
            _endpoint = endpoint;
            _timeout = timeout;
        }

        public void Open()
        {
            OpenConnection();
        }

        public void Close()
        {
            CloseConnection();
        }

        public void Send(string data)
        {
            _webSocket?.Send(data);
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseConnection();

                    _webSocket.OnOpen -= OnWebSocketOpened;
                    _webSocket.OnClose -= OnWebSocketClosed;
                    _webSocket.OnError -= OnWebSocketError;
                    _webSocket.OnMessage -= OnMessageReceived;

                    _socketOpenEvent.Dispose();
                    _socketCloseEvent.Dispose();
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

        ~WebSocketClient()
        {
            Dispose(false);
        }

        #endregion

    }
}
