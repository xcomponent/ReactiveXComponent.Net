
namespace ReactiveXComponent.Common
{
    public class WebSocketEndpoint
    {
        public WebSocketEndpoint()
        {
            
        }

        public WebSocketEndpoint(string name, string host, string port, WebSocketType type)
        {
            Name = name;
            Host = host;
            Port = port;
            Type = type;
        }

        public string Name { get; private set; }

        public string Host { get; set; }

        public string Port { get; set; }

        public WebSocketType Type { get; set; }

        public WebSocketEndpoint Clone()
        {
            return new WebSocketEndpoint(
                (string)Name?.Clone(), 
                (string)Host?.Clone(), 
                (string)Port?.Clone(),
                Type);
        }
    }
}
