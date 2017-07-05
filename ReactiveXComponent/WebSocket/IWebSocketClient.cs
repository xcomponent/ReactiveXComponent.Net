using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public interface IWebSocketClient : IDisposable
    {
        bool IsOpen { get; }
        void Init(WebSocketEndpoint endpoint, TimeSpan timeout, TimeSpan? retryInterval = null, int maxRetries = 5);
        void Open();
        void Close();
        void Send(string data);

        event EventHandler<EventArgs> ConnectionOpened;
        event EventHandler<EventArgs> ConnectionClosed;
        event EventHandler<System.IO.ErrorEventArgs> ConnectionError;
        event EventHandler<WebSocketMessageEventArgs> MessageReceived;
    }
}
