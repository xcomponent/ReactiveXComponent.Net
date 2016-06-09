using System;

namespace ReactiveXComponent.Connection
{
    public interface IXCConnection :IDisposable
    {
        IXCSession CreateSession();
    }
}