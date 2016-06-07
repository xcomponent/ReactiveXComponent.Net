using System;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.XCApi
{
    public interface IXCPublisher: IDisposable
    {
        void CreatePublisher(string component);
        void InitPrivateCommunication(string privateCommunicationIdentifier);
        void SendEvent(string engine, string component, string stateMachine, int eventCode, string messageType, object message, Visibility visibility);
        void Close();
        
    }
}