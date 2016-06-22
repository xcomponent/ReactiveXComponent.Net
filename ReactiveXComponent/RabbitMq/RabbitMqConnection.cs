using System;
using System.Threading;
using RabbitMQ.Client;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqConnection : IXCConnection
    {
        private bool _disposed;
        private IConnection _connection;
        private ConnectionFactory _factory;

        private readonly IXCConfiguration _xcConfiguration;

        public RabbitMqConnection(IXCConfiguration configuration)
        {
            _xcConfiguration = configuration;
            Init(configuration.GetBusDetails());
            _connection = CreateRabbitMqConnection();
        }

        private void Init(BusDetails busDetails)
        {
            try
            {
                _factory = new ConnectionFactory()
                {
                    UserName = busDetails.Username ?? XCApiTags.DefaultUserName,
                    Password = busDetails.Password ?? XCApiTags.DefaultPassword,
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

        private IConnection CreateRabbitMqConnection()
        {
            try
            {
                _connection = _factory?.CreateConnection();   
            }
            catch (CreateRabbitMqConnectionException ex)
            {
                throw new CreateRabbitMqConnectionException("Cannot connect to broker ", ex);
            }
            return _connection;
        }

        public IXCSession CreateSession(string privateCommunicationIdentifier = null)
        {
            return new RabbitMqSession(_xcConfiguration, _connection, privateCommunicationIdentifier);
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