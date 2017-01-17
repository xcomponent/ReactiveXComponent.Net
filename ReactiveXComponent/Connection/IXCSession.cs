using System;

namespace ReactiveXComponent.Connection
{
    public interface IXCSession : IDisposable
    {
        bool IsOpen { get; }
        event EventHandler SessionClosed;
        IXCPublisher CreatePublisher(string component);
        IXCSubscriber CreateSubscriber(string component);
    }
}