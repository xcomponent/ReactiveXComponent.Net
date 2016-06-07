using System;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public interface IXCConsumer: IDisposable
    {
        void CreateConsummer(string component, string stateMachine);
        void InitPrivateCommunication(string privateCommunicationIdentifier);
        void AddCallback(string component, string stateMachine, Action<MessageEventArgs> callback);
        void RemoveCallback(Action<MessageEventArgs> callback);
        void Close();

    }
}