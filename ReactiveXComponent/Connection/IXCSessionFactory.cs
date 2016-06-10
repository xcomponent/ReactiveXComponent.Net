using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public interface IXCSessionFactory
    {
        IXCSession CreateSession();
    }
}