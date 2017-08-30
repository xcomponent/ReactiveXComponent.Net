using System;

namespace ReactiveXComponent.Connection
{
    public interface IXCConnection
    {
        IXCSession CreateSession(TimeSpan? timeout = null, TimeSpan? retryInterval = null, int maxRetries = 5);
    }
}