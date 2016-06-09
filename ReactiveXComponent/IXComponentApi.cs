using System;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent
{
    public interface IXComponentApi: IDisposable
    {
        IXCSession CreateSession();
    }
}