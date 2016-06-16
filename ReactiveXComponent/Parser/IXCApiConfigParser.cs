using System.IO;

namespace ReactiveXComponent.Parser
{
    public interface IXCApiConfigParser
    {
        void Parse(Stream xcApiStream);
        string GetConnectionType();
        long GetComponentCode(string component);
        long GetStateMachineCode(string component, string stateMachine);
        int GetPublisherEventCode(string eventName);
        string GetPublisherTopic(string component, string stateMachine, int eventCode);
        string GetConsumerTopic(string component, string stateMachine);
    }
}