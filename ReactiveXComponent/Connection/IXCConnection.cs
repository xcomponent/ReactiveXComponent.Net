﻿using System;
using System.IO;

namespace ReactiveXComponent.Connection
{
    public interface IXCConnection :IDisposable
    {
        IXCSession CreateSession();
    }
}