using System.IO;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Configuration
{
    public interface IXCConfiguration
    {
        void Init(Stream xcApiStream);
        ConnectionType GetConnectionType();
        // TODO: replace these two methods with a GetCommunicationInfo() method
        BusDetails GetBusDetails();
        WebSocketEndpoint GetWebSocketEndpoint();
        int GetStateMachineCode(string component, string stateMachine);
        int GetComponentCode(string component);
        int GetStateCode(string component, string stateMachine, string state);
        int GetPublisherEventCode(string evnt);
        string GetPublisherTopic(string component, string stateMachine);
        string GetPublisherTopic(long componentCode, long stateMachineCode);
        string GetSubscriberTopic(string component, string stateMachine);
        string GetSerializationType();
        string GetSnapshotTopic(string component);
    }
}