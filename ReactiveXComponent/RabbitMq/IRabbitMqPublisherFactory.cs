
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public interface IRabbitMqPublisherFactory
    {
        IXCPublisher CreatePublisher();
    }
}