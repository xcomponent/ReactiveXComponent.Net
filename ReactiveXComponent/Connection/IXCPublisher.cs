using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void SendEvent(string stateMachine, object message, Visibility visibility);
        void SendEventWithStateMachineRef(StateMachineRefHeader stateMachineRefHeader, object message);
    }
}