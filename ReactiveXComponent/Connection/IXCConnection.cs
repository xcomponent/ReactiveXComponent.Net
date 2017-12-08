using System;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Connection
{
    public interface IXCConnection
    {
        IXCSession CreateSession(ConfigurationOverrides configurationOverrides = null);
    }
}