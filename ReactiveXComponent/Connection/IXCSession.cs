using System;
using System.Collections.Generic;

namespace ReactiveXComponent.Connection
{
    public interface IXCSession : IDisposable
    {
        bool IsOpen { get; }
        event EventHandler SessionClosed;
        IXCPublisher CreatePublisher(string component);
        IXCSubscriber CreateSubscriber(string component);
        List<string> GetXCApiList(TimeSpan? timeout = null);
        string GetXCApi(string apiFullName, TimeSpan? timeout = null);
    }
}