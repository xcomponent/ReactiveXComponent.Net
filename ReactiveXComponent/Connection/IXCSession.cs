﻿using System;

namespace ReactiveXComponent.Connection
{
    public interface IXCSession : IDisposable
    {
        IXCPublisher CreatePublisher(string component);
        IXCSubscriber CreateSubscriber(string component);
    }
}