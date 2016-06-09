using System;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisherFactory : IDisposable
    {
        IXCPublisher CreatePublisher(string component);
    }
}