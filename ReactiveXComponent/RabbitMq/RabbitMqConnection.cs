using System;
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
        private readonly string _privateCommunicationIdentifier;

        public RabbitMqConnection(IXCConfiguration configuration, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = configuration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
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
            catch (ReactiveXComponentException e)
            {
                throw new ReactiveXComponentException("RabbitMQ Connection init failed" + e.Message, e);
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
                throw new CreateRabbitMqConnectionException("Cannot connect to broker" + ex.Message, ex);
            }
            return _connection;
        }

        public IXCSession CreateSession()
        {
            return new RabbitMqSession(_xcConfiguration, _connection, _privateCommunicationIdentifier);
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
        }
    }
}