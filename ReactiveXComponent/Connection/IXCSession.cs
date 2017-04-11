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
        List<string> GetXCApiList(int timeout = 10000);
        string GetXCApi(string apiFullName, int timeout = 10000);
    }
}