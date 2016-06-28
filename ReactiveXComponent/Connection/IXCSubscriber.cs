using System;
using ReactiveXComponent.RabbitMq;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.Connection
{
    public interface IXCSubscriber : IDisposable
    {
        void AddCallback(string component, string stateMachine, Action<MessageEventArgs> callback);
        void RemoveCallback(string component, string stateMachine, Action<MessageEventArgs> callback);
    }
}
