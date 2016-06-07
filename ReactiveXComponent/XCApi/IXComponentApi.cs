using System;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public interface IXComponentApi : IDisposable
    {
        void CreateSession();
        void Close();
    }
}