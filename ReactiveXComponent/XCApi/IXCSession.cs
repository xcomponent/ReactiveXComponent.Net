using System;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public interface IXCSession: IDisposable
    {
        void CreatePublisher(string component);
        void CreateConsumer(string component, string stateMachine);
        void Close();
    }
}