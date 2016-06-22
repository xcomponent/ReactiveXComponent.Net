using System.IO;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private readonly IXCConnection _xcConnection;

        private string _privateCommunicationIdentifier;

        private XComponentApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            var parser = new XCApiConfigParser();
            var xcConfiguration = new XCConfiguration(parser);
            xcConfiguration.Init(xcApiStream);
            AbstractXCConnectionFactory connectionFactory = new XCConnectionFactory(xcConfiguration);
            _xcConnection = connectionFactory.CreateConnection();
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public static IXComponentApi CreateFromXCApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            return new XComponentApi(xcApiStream);
        }

        public IXCSession CreateSession()
        {
            return _xcConnection.CreateSession(_privateCommunicationIdentifier);
        }
    }
}
