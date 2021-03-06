﻿using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent
{
    public interface IXComponentApi
    {
        IXCSession CreateSession(ConfigurationOverrides configurationOverrides = null);
    }
}