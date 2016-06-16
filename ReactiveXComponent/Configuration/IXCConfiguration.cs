using System.IO;

namespace ReactiveXComponent.Configuration
{
    public interface IXCConfiguration
    {
        void Init(Stream xcApiStream);
        string GetConnectionType();
        long GetStateMachineCode(string component, string stateMachine);
        long GetComponentCode(string component);
        int GetPublisherEventCode(string evnt);
    }
}