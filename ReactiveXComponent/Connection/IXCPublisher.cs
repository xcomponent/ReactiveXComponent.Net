using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher : IDisposable
    {
        void InitPrivateCommunication(string privateCommunicationIdentifier);
        void SendEvent(string engine, string component, string stateMachine, int eventCode, string messageType, object message, Visibility visibility);
    }
}