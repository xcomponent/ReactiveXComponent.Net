namespace XCClientLib.RabbitMQ
{
    using System;

    public interface IConsumer
    {
        event EventHandler<MessageEventArgs> MessageReceived;

        void Start();

        void Stop();
    }
}