using System.IO;

namespace ReactiveXComponent.Configuration
{
    public interface IXCConfiguration
    {
        void Init(Stream xcApiStream);
        string GetConnectionType();
        BusDetails GetBusDetails();
        long GetStateMachineCode(string component, string stateMachine);
        long GetComponentCode(string component);
        int GetPublisherEventCode(string evnt);
        string GetPublisherTopic(string component, string stateMachine);
        string GetSubscriberTopic(string component, string stateMachine);
        string GetSerializationType();
    }
}