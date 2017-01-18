using System.ComponentModel;
using XCClientLib.Common;

namespace XCClientLib
{
    using System;

    using XCClientLib.RabbitMQ;

    public interface IXComponentApi : IDisposable
    {
        void SendEvent(string engine, string component, string stateMachine, int eventCode, string messageType, object message, Visibility visibility);

        void AddCallback(string component, string stateMachine, Action<MessageEventArgs> callback);

        void RemoveCallback(Action<MessageEventArgs> callback);

        void Close();

        void InitPrivateConsumer(string component, string stateMachine);
    }
}