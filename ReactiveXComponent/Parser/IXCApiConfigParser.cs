using System.IO;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Parser
{
    public interface IXCApiConfigParser
    {
        void Parse(Stream xcApiStream);
        string GetConnectionType();
        BusDetails GetBusDetails();
        WebSocketEndpoint GetWebSocketEndpoint();
        int GetComponentCode(string component);
        int GetStateMachineCode(string component, string stateMachine);
        int GetPublisherEventCode(string eventName);
        string GetPublisherTopic(string component, string stateMachine);
        string GetPublisherTopic(long componentCode, long stateMachineCode);
        string GetSubscriberTopic(string component, string stateMachine);
        string GetSerializationType();
        string GetSnapshotTopic(string component);
    }
}