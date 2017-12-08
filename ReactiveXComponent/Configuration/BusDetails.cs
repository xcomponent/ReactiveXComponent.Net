
namespace ReactiveXComponent.Configuration
{
    public class BusDetails
    {
        public BusDetails()
        {
            
        }

        public BusDetails(string username, string password, string host, int port)
        {
            Username = username;
            Password = password;
            Host = host;
            Port = port;
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public BusDetails Clone()
        {
            return new BusDetails(
                (string)Username?.Clone(),
                (string)Password?.Clone(),
                (string)Host?.Clone(),
                Port);
        }
    }
}
