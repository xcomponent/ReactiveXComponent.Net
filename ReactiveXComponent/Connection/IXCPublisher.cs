using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void SendEvent(string stateMachine, object message, Visibility visibility = Visibility.Public);
        void SendEvent(StateMachineRefHeader stateMachineRefHeader, object message, Visibility visibility = Visibility.Public);
    }
}