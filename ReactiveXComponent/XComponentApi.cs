using System;
using System.IO;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private readonly IXCConnection _xcConnection;

        private XComponentApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            var parser = new XCApiConfigParser();
            var xcConfiguration = new XCConfiguration(parser);
            xcConfiguration.Init(xcApiStream);
            AbstractXCConnectionFactory connectionFactory = new XCConnectionFactory(xcConfiguration, privateCommunicationIdentifier);
            var connectionType = xcConfiguration.GetConnectionType();
            _xcConnection = connectionFactory.CreateConnection(connectionType);
        }

        public static IXComponentApi CreateFromXCApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            return new XComponentApi(xcApiStream, privateCommunicationIdentifier);
        }

        public IXCSession CreateSession(TimeSpan? timeout = null, TimeSpan? retryInterval = null, int maxRetries = 5)
        {
            return _xcConnection.CreateSession(timeout, retryInterval, maxRetries);
        }
    }
}
