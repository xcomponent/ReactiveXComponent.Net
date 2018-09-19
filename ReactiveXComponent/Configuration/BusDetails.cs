
namespace ReactiveXComponent.Configuration
{
    public class BusDetails
    {
        public BusDetails()
        {
            
        }

        public BusDetails(string username, string password, string host, string virtualHost, int port)
        {
            Username = username;
            Password = password;
            Host = host;
            VirtualHost = virtualHost;
            Port = port;

        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }

        public string VirtualHost { get; set; }

        public int Port { get; set; }

        public BusDetails Clone()
        {
            return new BusDetails(
                Username,
                Password,
                Host,
                VirtualHost,
                Port);
        }
    }
}
