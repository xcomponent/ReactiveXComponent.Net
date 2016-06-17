using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void SendEvent( string component, string stateMachine, object message, Visibility visibility);
    }
}