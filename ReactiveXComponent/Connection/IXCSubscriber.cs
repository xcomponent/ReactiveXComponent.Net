using System;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent.Connection
{
    public interface IXCSubscriber : IDisposable
    {
        void Subscribe(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener);
        void Unsubscribe(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener);
    }
}
