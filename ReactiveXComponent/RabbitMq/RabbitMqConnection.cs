using System;
using System.Threading;
using RabbitMQ.Client;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqConnection : IXCConnection
    {
        private const string CannotReachBroker = "Cannot connect to broker ";

        private const int TimeoutConnection = 10000;

        private bool _disposed;
        private IConnection _connection;
        private ConnectionFactory _factory;

        private readonly IXCConfiguration _xcConfiguration;

        public RabbitMqConnection(IXCConfiguration configuration)
        {
            _xcConfiguration = configuration;
            Init(configuration.GetBusDetails());
            _connection = CreateConnection();
        }

        private void Init(BusDetails busDetails)
        {
            try
            {
                _factory = new ConnectionFactory()
                {
                    UserName = busDetails.Username ?? "guest",
                    Password = busDetails.Password ?? "guest",
                    VirtualHost = ConnectionFactory.DefaultVHost,
                    HostName = busDetails.Host,
                    Port = busDetails.Port,
                    Protocol = Protocols.DefaultProtocol
                };
            }
            catch (Exception e)
            {
                throw new Exception("RabbitMQ Connection init failed", e);
            }
        }

        private IConnection CreateConnection()
        {
            if (_factory != null)
            {
                var lockEvent = new AutoResetEvent(false);
                string errorMessage = null;
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    try
                    {
                        _connection = _factory.CreateConnection();
                        lockEvent.Set();
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message;
                    }
                });
                if (!lockEvent.WaitOne(TimeoutConnection))
                {
                    throw new Exception(CannotReachBroker, null);
                }

                if (errorMessage != null)
                {
                    throw new Exception(errorMessage);
                }
            }
            return _connection;
        }

        public IXCSession CreateSession()
        {
            return new RabbitMqSession(_xcConfiguration, _connection);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _connection?.Close();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}