using XComponent.Shared.Failover;

namespace XComponent.HelloWorld.UserObject
{
    public class ConnectionStateChanged 
    {
        public bool IsPublic => false;
        public bool IsSerializable => false;

        public ConnectionStateChanged(string sqlConnectionState)
        {
            SqlConnectionState = sqlConnectionState;
        }

        public string SqlConnectionState { get; }

    }
}
