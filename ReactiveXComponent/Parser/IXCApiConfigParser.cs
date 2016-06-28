using System.IO;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Parser
{
    public interface IXCApiConfigParser
    {
        void Parse(Stream xcApiStream);
        string GetConnectionType();
        BusDetails GetBusDetails();
        long GetComponentCode(string component);
        long GetStateMachineCode(string component, string stateMachine);
        int GetPublisherEventCode(string eventName);
        string GetPublisherTopic(string component, string stateMachine, int eventCode);
        string GetSubscriberTopic(string component, string stateMachine);
    }
}