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

            if (!string.IsNullOrEmpty(configurationOverrides.VirtualHost))
            {
                busDetails.VirtualHost = configurationOverrides.VirtualHost;
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

            if (configurationOverrides.SslEnabled != null)
            {
                busDetails.SslEnabled = configurationOverrides.SslEnabled.Value;
            }

            if (configurationOverrides.SslServerName != null)
            {
                busDetails.SslServerName = configurationOverrides.SslServerName;
            }

            if (configurationOverrides.SslCertificatePath != null)
            {
                busDetails.SslCertificatePath = configurationOverrides.SslCertificatePath;
            }

            if (configurationOverrides.SslCertificatePassphrase != null)
            {
                busDetails.SslCertificatePassphrase = configurationOverrides.SslCertificatePassphrase;
            }

            if (configurationOverrides.SslProtocol != null)
            {
                busDetails.SslProtocol = configurationOverrides.SslProtocol.Value;
            }

            if (configurationOverrides.SslAllowUntrustedServerCertificate != null)
            {
                busDetails.SslAllowUntrustedServerCertificate = configurationOverrides.SslAllowUntrustedServerCertificate.Value;
            }

            return new RabbitMqSession(_xcConfiguration, busDetails, _privateCommunicationIdentifier);
        }
    }
}