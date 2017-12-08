using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqConnection : IXCConnection
    {
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly BusDetails _busDetails;

        public RabbitMqConnection(IXCConfiguration configuration, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = configuration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _busDetails = configuration?.GetBusDetails();
        }

        public IXCSession CreateSession(ConfigurationOverrides configurationOverrides = null)
        {
            if (configurationOverrides == null)
            {
                return new RabbitMqSession(_xcConfiguration, _busDetails, _privateCommunicationIdentifier);
            }

            var busDetails = _busDetails.Clone();

            if (configurationOverrides.Host != null)
            {
                busDetails.Host = configurationOverrides.Host;
            }

            if (configurationOverrides.Port != null)
            {
                busDetails.Port = int.Parse(configurationOverrides.Port);
            }

            if (configurationOverrides.Username != null)
            {
                busDetails.Username = configurationOverrides.Username;
            }

            if (configurationOverrides.Password != null)
            {
                busDetails.Password = configurationOverrides.Password;
            }

            return new RabbitMqSession(_xcConfiguration, busDetails, _privateCommunicationIdentifier);
        }
    }
}