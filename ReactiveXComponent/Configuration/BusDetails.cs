
using System.Security.Authentication;

namespace ReactiveXComponent.Configuration
{
    public class BusDetails
    {
        public BusDetails()
        {
            
        }

        public BusDetails(
            string username, 
            string password, 
            string host, 
            string virtualHost, 
            int port,
            bool sslEnabled = false,
            string sslServerName = "",
            string sslCertificatePath = "",
            string sslCertificatePassphrase = "",
            SslProtocols sslProtocol = SslProtocols.Default,
            bool sslAllowUntrustedServerCertificate = false)
        {
            Username = username;
            Password = password;
            Host = host;
            VirtualHost = virtualHost;
            Port = port;
            SslEnabled = sslEnabled;
            SslServerName = sslServerName;
            SslCertificatePath = sslCertificatePath;
            SslCertificatePassphrase = sslCertificatePassphrase;
            SslProtocol = sslProtocol;
            SslAllowUntrustedServerCertificate = sslAllowUntrustedServerCertificate;
        }

        /// <summary>
        /// Rabbit Mq user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Rabbit Mq password for user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Rabbit Mq server's address.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Rabbit Mq virtual host to connect to. 
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// Rabbit Mq server's port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// To enable SSL.
        /// </summary>
        public bool SslEnabled { get; set; }
        
        /// <summary>
        /// Server's Common Name. It's indicated in the CN field of the server's certificate.
        /// </summary>
        public string SslServerName { get; set; }
        
        /// <summary>
        /// Path to the client's certificate.
        /// </summary>
        public string SslCertificatePath { get; set; }

        /// <summary>
        /// Passphrase for the client's certificate if it has one.
        /// </summary>
        public string SslCertificatePassphrase { get; set; }

        /// <summary>
        /// SSL protocol to use.
        /// </summary>
        public SslProtocols SslProtocol { get; set; }

        /// <summary>
        /// To accept untrusted (e.g self-signed) server certificates. Only use this in Dev environment.
        /// </summary>
        public bool SslAllowUntrustedServerCertificate { get; set; }

        public BusDetails Clone()
        {
            return new BusDetails(
                Username,
                Password,
                Host,
                VirtualHost,
                Port,
                SslEnabled,
                SslServerName,
                SslCertificatePath,
                SslCertificatePassphrase,
                SslProtocol,
                SslAllowUntrustedServerCertificate);
        }
    }
}
