using System;

namespace ReactiveXComponent.Connection
{
    public interface IXCConnection
    {
        IXCSession CreateSession();
    }
}