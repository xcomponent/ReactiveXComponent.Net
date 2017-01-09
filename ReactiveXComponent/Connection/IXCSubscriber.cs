using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCSubscriber : IDisposable
    {
        void Subscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener);
        void Unsubscribe(string stateMachine, Action<MessageEventArgs> stateMachineListener);
        IObservable<MessageEventArgs> StateMachineUpdatesStream { get; }
    }
}
