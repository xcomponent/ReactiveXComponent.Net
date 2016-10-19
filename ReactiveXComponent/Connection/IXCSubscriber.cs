using System;
using System.Collections.Generic;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent.Connection
{
    public interface IXCSubscriber : IDisposable
    {
        void Subscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener);
        void Unsubscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener);
        IObservable<MessageEventArgs> StateMachineUpdatesStream { get; }
        void GetSnapshot(string stateMachine, Action<MessageEventArgs> OnGetSnapshot);
    }
}
