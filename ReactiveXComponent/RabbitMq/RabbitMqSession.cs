using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSession : IXCSession
    {
        private readonly IXCConfiguration _xcConfiguration;
        private IConnection _connection;
        private readonly string _privateCommunicationIdentifier;
        private readonly ISerializer _serializer;
        private ConnectionFactory _factory;

        public RabbitMqSession(IXCConfiguration xcConfiguration, BusDetails busDetails , string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializer = SelectSerializer();
            InitConnection(busDetails);
        }

        private void InitConnection(BusDetails busDetails)
        {
            try
            {
                _factory = new ConnectionFactory()
                {
                    UserName = busDetails.Username ?? XCApiTags.DefaultUserName,
                    Password = busDetails.Password ?? XCApiTags.DefaultPassword,
                    VirtualHost = string.IsNullOrEmpty(busDetails.VirtualHost) ? ConnectionFactory.DefaultVHost : busDetails.VirtualHost,
                    HostName = busDetails.Host,
                    Port = busDetails.Port,
                    Protocol = Protocols.DefaultProtocol
                };

                _connection = _factory?.CreateConnection();

                _connection.ConnectionShutdown += ConnectionOnConnectionShutdown;
            }
            catch (BrokerUnreachableException e)
            {
                throw new ReactiveXComponentException("Error while creating Rabbit Mq connection: " + e.Message, e);
            }
        }

        private void ConnectionOnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }

        private ISerializer SelectSerializer()
        {
            switch (_xcConfiguration.GetSerializationType())
            {
                case XCApiTags.Binary:
                    return SerializerFactory.CreateSerializer(SerializationType.Binary);
                case XCApiTags.Json:
                    return SerializerFactory.CreateSerializer(SerializationType.Json);
                case XCApiTags.Bson:
                    return SerializerFactory.CreateSerializer(SerializationType.Bson);
                default:
                    throw new XCSerializationException("Serialization type not supported");
            }
        }

        public bool IsOpen => _connection.IsOpen;

        public event EventHandler SessionClosed;

        public IXCPublisher CreatePublisher(string component)
        {
            return new RabbitMqPublisher(component, _xcConfiguration, _connection, _serializer, _privateCommunicationIdentifier);
        }
   
        public IXCSubscriber CreateSubscriber(string component)
        {
            return new RabbitMqSubscriber(component, _xcConfiguration, _connection, _serializer, _privateCommunicationIdentifier);
        }

        public List<string> GetXCApiList(string requestId = null, TimeSpan? timeout = null)
        {
            throw new NotImplementedException("Method not supported for Rabbit MQ");
        }

        public string GetXCApi(string apiFullName, string requestId = null, TimeSpan? timeout = null)
        {
            throw new NotImplementedException("Method not supported for Rabbit MQ");
        }

        private void CloseConnection()
        {
            if (_connection == null) return;

            _connection.ConnectionShutdown -= ConnectionOnConnectionShutdown;
            _connection.Dispose();
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseConnection();
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

        ~RabbitMqSession()
        {
            Dispose(false);
        }

        #endregion
    }
}