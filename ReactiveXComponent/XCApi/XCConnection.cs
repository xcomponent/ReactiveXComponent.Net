using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public class XCConnection : IXCConnection
    {
        private bool _disposed = false;

        private readonly IRabbitMqConnection _connection;

        public XCConnection(IRabbitMqConnection connection)
        {
            _connection = connection;
        }

        public void Close()
        {
            _connection?.Close();
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
                return;
            else
            {
                Close();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(_disposed);
            GC.SuppressFinalize(this);
        }
    }
}
