using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisher
    {
        void SendEvent( string component, string stateMachine, object message, Visibility visibility);
    }
}