namespace ReactiveXComponent.RabbitMQ
{
    public class BusDetails
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public override bool Equals(object obj)
        {
            var toCompareWith = obj as BusDetails;
        
            return toCompareWith != null && Equals(toCompareWith);
        }
        
        protected bool Equals(BusDetails other)
        {
            return string.Equals(Username, other.Username) && string.Equals(Password, other.Password) && string.Equals(Host, other.Host) && Port == other.Port;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Username != null ? Username.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Host != null ? Host.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Port;
                return hashCode;
            }
        }
    }
}
