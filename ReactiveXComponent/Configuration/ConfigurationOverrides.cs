using System.Security.Authentication;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Configuration
{
    public class ConfigurationOverrides
    {
        public string Host { get; set; }

        public string VirtualHost { get; set; }

        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public WebSocketType? WebSocketType { get; set; }

        public bool? SslEnabled { get; set; }

        public string SslServerName { get; set; }

        public string SslCertPath { get; set; }

        public string SslCertPassphrase { get; set; }

        public SslProtocols? SslProtocol { get; set; }

        public bool? SslAllowUntrustedServerCertificate { get; set; }
    }
}
