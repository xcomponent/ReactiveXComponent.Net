﻿namespace ReactiveXComponent.Connection
{
    public interface IXCConnection
    {
        IXCSession CreateSession();
        void Close();
    }
}