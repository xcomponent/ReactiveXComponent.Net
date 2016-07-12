using System;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.Connection
{
    public interface IXCSubscriber : IDisposable
    {
        void Subscribe(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener);
        void DeleteStateMachineListener(string component, string stateMachine, Action<MessageEventArgs> stateMachineListener);
        IObservable<MessageEventArgs> GetStateMachineUpdates();
    }
}
