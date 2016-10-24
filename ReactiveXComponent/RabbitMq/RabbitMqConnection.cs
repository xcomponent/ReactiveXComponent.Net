using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqConnection : IXCConnection
    {
        private IConnection _connection;
        private ConnectionFactory _factory;
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;

        public RabbitMqConnection(IXCConfiguration configuration, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = configuration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;

            Init(configuration.GetBusDetails());
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

                _connection = _factory?.CreateConnection();
            }
            catch (BrokerUnreachableException e)
            {
                throw new ReactiveXComponentException("Error while creating Rabbit Mq connection: " + e.Message, e);
            }
            
        }

        public IXCSession CreateSession()
        {
            return new RabbitMqSession(_xcConfiguration, _connection, _privateCommunicationIdentifier);
        }
        
        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }

                // clear unmanaged resources

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RabbitMqConnection()
        {
            Dispose(false);
        }

        #endregion

    }
}