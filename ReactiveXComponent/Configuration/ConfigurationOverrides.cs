using ReactiveXComponent.Common;

namespace ReactiveXComponent.Configuration
{
    public class ConfigurationOverrides
    {
        public string Host { get; set; }

        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public WebSocketType? WebSocketType { get; set; }

        public bool EmptyStringAsEmptyValue { get; set; } = true;

        public bool HostHasValue()
        {
            return !IsFieldEmpty(Host);
        }

        public bool PortHasValue()
        {
            return !IsFieldEmpty(Port);
        }

        public bool UsernameHasValue()
        {
            return !IsFieldEmpty(Username);
        }

        public bool PasswordHasValue()
        {
            return !IsFieldEmpty(Password);
        }

        public bool WebSocketTypeHasValue()
        {
            return WebSocketType.HasValue;
        }

        private bool IsFieldEmpty(string field)
        {
            if (EmptyStringAsEmptyValue)
            {
                return string.IsNullOrEmpty(field);
            }

            return field == null;
        }
    }
}
