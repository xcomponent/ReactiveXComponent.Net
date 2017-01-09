using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public interface IWebSocketClient : IDisposable
    {
        bool IsOpen { get; }
        void Init(WebSocketEndpoint endpoint, int timeout);
        void Open();
        void Close();
        void Send(string data);

        event EventHandler<EventArgs> ConnectionOpened;
        event EventHandler<EventArgs> ConnectionClosed;
        event EventHandler<System.IO.ErrorEventArgs> ConnectionError;
        event EventHandler<WebSocketSharp.MessageEventArgs> MessageReceived;
    }
}
