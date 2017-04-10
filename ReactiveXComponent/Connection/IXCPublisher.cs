using System;
using System.Collections.Generic;
using System.IO;
using ReactiveXComponent.Common;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void SendEvent(string stateMachine, object message, Visibility visibility = Visibility.Public);
        void SendEvent(StateMachineRefHeader stateMachineRefHeader, object message, Visibility visibility = Visibility.Public);
        List<MessageEventArgs> GetSnapshot(string stateMachine, int timeout = 10000);
        void GetSnapshotAsync(string stateMachine, Action<List<MessageEventArgs>> onSnapshotReceived);
        List<string> GetXCApiNames(int timeout = 10000);
        Stream GetXCApi(string apiFullName, int timeout = 10000);
    }
}