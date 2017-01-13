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
            _xcConnection = connectionFactory.CreateConnection();
        }

        public static IXComponentApi CreateFromXCApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            return new XComponentApi(xcApiStream, privateCommunicationIdentifier);
        }

        public IXCSession CreateSession()
        {
            return _xcConnection.CreateSession();
        }
    }
}
