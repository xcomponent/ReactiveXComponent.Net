using System.IO;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private readonly IXCConnection _xcConnection;

        public static string PrivateCommunicationIdentifier;

        private XComponentApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            var parser = new XCApiConfigParser();
            var xcConfiguration = new XCConfiguration(parser);
            xcConfiguration.Init(xcApiStream);
            AbstractXCConnectionFactory connectionFactory = new XCConnectionFactory(xcConfiguration);
            _xcConnection = connectionFactory.CreateConnection();
            PrivateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public static IXComponentApi CreateFromXCApi(Stream xcApiStream)
        {
            return new XComponentApi(xcApiStream);
        }

        public IXCSession CreateSession()
        {
            return _xcConnection.CreateSession();
        }
    }
}
