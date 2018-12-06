
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
            string sslCertPath = "",
            string sslCertPassphrase = "",
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
            SslCertPath = sslCertPath;
            SslCertPassphrase = sslCertPassphrase;
            SslProtocol = sslProtocol;
            SslAllowUntrustedServerCertificate = sslAllowUntrustedServerCertificate;
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }

        public string VirtualHost { get; set; }

        public int Port { get; set; }

        public bool SslEnabled { get; set; }
        
        public string SslServerName { get; set; }
        
        public string SslCertPath { get; set; }

        public string SslCertPassphrase { get; set; }

        public SslProtocols SslProtocol { get; set; }

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
                SslCertPath,
                SslCertPassphrase,
                SslProtocol,
                SslAllowUntrustedServerCertificate);
        }
    }
}
