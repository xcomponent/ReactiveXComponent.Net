using System;

namespace ReactiveXComponent.Connection
{
    public interface IXCPublisherFactory
    {
        IXCPublisher CreatePublisher();
    }
}