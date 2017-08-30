using System;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent
{
    public interface IXComponentApi
    {
        IXCSession CreateSession(TimeSpan? timeout = null, TimeSpan? retryInterval = null, int maxRetries = 5);
    }
}