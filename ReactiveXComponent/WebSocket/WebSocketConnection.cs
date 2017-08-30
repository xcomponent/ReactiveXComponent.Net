using System;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketConnection : IXCConnection
    {
        private readonly WebSocketEndpoint _endpoint;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        public WebSocketConnection(IXCConfiguration xcConfiguration, WebSocketEndpoint endpoint, string privateCommunicationIdentifier = null)
        {
            _endpoint = endpoint;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public IXCSession CreateSession(TimeSpan? timeout = null, TimeSpan? retryInterval = null, int maxRetries = 5)
        {
            return new WebSocketSession(_endpoint, _xcConfiguration, _privateCommunicationIdentifier, timeout, retryInterval, maxRetries);
        }
    }
}
