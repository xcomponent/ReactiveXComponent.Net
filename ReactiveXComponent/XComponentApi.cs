using System.IO;
using ReactiveXComponent.Common;
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

        public static IXComponentApi CreateFromXCApi(string xcApiFilePath, string privateCommunicationIdentifier = null)
        {
            if (!File.Exists(xcApiFilePath))
            {
                throw new ReactiveXComponentException($"The file {xcApiFilePath} doesn't exist");
            }

            using (var stream = new FileStream(xcApiFilePath, FileMode.Open))
            {
                return CreateFromXCApi(stream, privateCommunicationIdentifier);
            }
        }

        public static IXComponentApi CreateFromXCApi(Stream xcApiStream, string privateCommunicationIdentifier = null)
        {
            return new XComponentApi(xcApiStream, privateCommunicationIdentifier);
        }

        public IXCSession CreateSession(ConfigurationOverrides configurationOverrides = null)
        {
            return _xcConnection.CreateSession(configurationOverrides);
        }
    }
}
