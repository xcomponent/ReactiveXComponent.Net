using System;
using RabbitMQ.Client;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void SendEvent(string stateMachine, object message, Visibility visibility);
        IModel PublisherChanne { get; }
    }
}