using System;
using System.Collections.Generic;
using ReactiveXComponent.Common;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void SendEvent(string stateMachine, object message, Visibility visibility = Visibility.Public);
        void SendEvent(StateMachineRefHeader stateMachineRefHeader, object message, Visibility visibility = Visibility.Public);
        IObservable<object> GetSnapshot(string stateMachine);
    }
}