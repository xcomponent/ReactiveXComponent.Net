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

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketSession : IXCSession
    {
        private readonly IWebSocketClient _webSocketClient;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        public WebSocketSession(IWebSocketClient webSocketClient, IXCConfiguration xcConfiguration, string privateCommunicationIdentifier)
        {
            _webSocketClient = webSocketClient;
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public IXCPublisher CreatePublisher(string component)
        {
            throw new NotImplementedException();
        }

        public IXCSubscriber CreateSubscriber(string component)
        {
            return new WebSocketSubscriber(component, _webSocketClient, _xcConfiguration, _privateCommunicationIdentifier);
        }
    }
}
