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

        public IXCSession CreateSession(ConfigurationOverrides configurationOverrides = null)
        {
            if (configurationOverrides == null)
            {
                return new WebSocketSession(_endpoint, _timeout, _xcConfiguration, _privateCommunicationIdentifier);
            }

            var endpoint = _endpoint.Clone();

            if (configurationOverrides.Host != null)
            {
                endpoint.Host = configurationOverrides.Host;
            }

            if (configurationOverrides.Port != null)
            {
                endpoint.Port = configurationOverrides.Port;
            }

            if (configurationOverrides.WebSocketType != null)
            {
                endpoint.Type = configurationOverrides.WebSocketType.Value;
            }

            return new WebSocketSession(endpoint, _timeout, _xcConfiguration, _privateCommunicationIdentifier);
        }
    }
}
