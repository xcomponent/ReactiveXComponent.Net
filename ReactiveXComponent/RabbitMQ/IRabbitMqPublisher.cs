using System;

namespace ReactiveXComponent.RabbitMQ
{
    public interface IRabbitMqPublisher: IDisposable
    {
        void Send(Header header, object message, string routingKey);
        void Close();
    }
}