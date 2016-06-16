using ReactiveXComponent.Connection;

namespace ReactiveXComponent.Connection
{
    public interface IXCSession
    {
        IXCPublisher CreatePublisher();
    }
}