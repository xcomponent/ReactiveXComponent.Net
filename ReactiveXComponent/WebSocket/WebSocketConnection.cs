using System;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketConnection : IXCConnection
    {
        private readonly WebSocketEndpoint _endpoint;
        private readonly int _timeout;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        public WebSocketConnection(IXCConfiguration xcConfiguration, WebSocketEndpoint endpoint, int timeout, string privateCommunicationIdentifier = null)
        {
            _endpoint = endpoint;
            _timeout = timeout;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public IXCSession CreateSession()
        {
            return new WebSocketSession(_endpoint, _timeout, _xcConfiguration, _privateCommunicationIdentifier);
        }
    }
}
