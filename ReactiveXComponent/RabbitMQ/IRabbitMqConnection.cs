namespace ReactiveXComponent.RabbitMQ
{
    using System;

    using global::RabbitMQ.Client;

    public interface IRabbitMqConnection : IDisposable
    {
        void Close();

        IConnection GetConnection();
    }
}