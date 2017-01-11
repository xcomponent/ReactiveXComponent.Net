using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public abstract class AbstractXCConnectionFactory
    {
        public abstract IXCConnection CreateConnection(ConnectionType connectionType, int connectionTimeout = 10000);
    }
}
