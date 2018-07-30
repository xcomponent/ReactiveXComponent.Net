using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void SendEvent(string stateMachine, object message, string messageType, Visibility visibility = Visibility.Public);
        void SendEvent(string stateMachine, object message, Visibility visibility = Visibility.Public);
        void SendEvent(StateMachineRefHeader stateMachineRefHeader, object message, Visibility visibility = Visibility.Public);
        List<MessageEventArgs> GetSnapshot(string stateMachine, int? chunkSize = null, int timeout = 10000);
        Task<List<MessageEventArgs>> GetSnapshotAsync(string stateMachine, int? chunkSize = null, int timeout = 10000);
    }
}