using System.IO;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private readonly IXCConnection _xcConnection;
        private readonly XCConfiguration _xcConfiguration;

        public static string PrivateCommunicationIdentifier;

        private XComponentApi(Stream xcApiStream)
        {
            var parser = new XCApiConfigParser();
            _xcConfiguration = new XCConfiguration(parser);
            _xcConfiguration.Init(xcApiStream);
            AbstractXCConnectionFactory connectionFactory = new XCConnectionFactory(_xcConfiguration);
            _xcConnection = connectionFactory.CreateConnection();
        }

        public static void InitPrivateCommunivation(string privateCommunicationIdentifier)
        {
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
