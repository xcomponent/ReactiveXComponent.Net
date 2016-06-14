using System.IO;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private readonly IXCConnection _xcConnection;
        private readonly XCConfiguration _xcConfiguration;

        private XComponentApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            var parser = new Parser.Parser(xcApiStream);
            _xcConfiguration = new XCConfiguration(parser);
            _xcConfiguration.Init();
            var connectionFactory = new XCConnectionFactory(_xcConfiguration);
            _xcConnection = connectionFactory.CreateConnection();
            XCPublisher.PrivateCommunicationIdentifier = privateCommunicationIdentifier;
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
