using System;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent
{
    public interface IXComponentApi
    {
        IXCSession CreateSession();
    }
}