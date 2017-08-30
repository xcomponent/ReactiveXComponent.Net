using System;
using System.Collections.Generic;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
        private readonly TimeSpan? _timeout;
        private readonly TimeSpan? _retryInterval;

        public RabbitMqSession(IXCConfiguration xcConfiguration, BusDetails busDetails , string privateCommunicationIdentifier = null, TimeSpan? timeout = null, TimeSpan? retryInterval = null)
        {
            _xcConfiguration = xcConfiguration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _serializer = SelectSerializer();
            _timeout = timeout;
            _retryInterval = retryInterval;
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
                    VirtualHost = ConnectionFactory.DefaultVHost,
                    HostName = busDetails.Host,
                    Port = busDetails.Port,
                    Protocol = Protocols.DefaultProtocol,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = false,
                    NetworkRecoveryInterval = (_retryInterval != null) ? _retryInterval.Value : TimeSpan.FromSeconds(5),
                    RequestedConnectionTimeout = (_timeout != null) ? (int)_timeout.Value.TotalMilliseconds : 10000
                };

                _connection = _factory?.CreateConnection();
                SessionOpened?.Invoke(this, EventArgs.Empty);
                _connection.ConnectionUnblocked += ConnectionOnConnectionUnblocked;
                _connection.ConnectionShutdown += ConnectionOnConnectionShutdown;
                _connection.ConnectionBlocked += ConnectionOnConnectionBlocked;
            }
            catch (BrokerUnreachableException e)
            {
                throw new ReactiveXComponentException("Error while creating Rabbit Mq connection: " + e.Message, e);
            }
        }

        private void ConnectionOnConnectionUnblocked(object sender, EventArgs eventArgs)
        {
            SessionOpened?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectionOnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectionOnConnectionBlocked(object sender, ConnectionBlockedEventArgs connectionBlockedEventArgs)
        {
            ConnectionError?.Invoke(this, new ErrorEventArgs(new Exception(connectionBlockedEventArgs.Reason)));
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

        public event EventHandler SessionOpened;

        public event EventHandler SessionClosed;

        public event EventHandler<System.IO.ErrorEventArgs> ConnectionError;

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

            _connection.ConnectionUnblocked -= ConnectionOnConnectionUnblocked;
            _connection.ConnectionShutdown -= ConnectionOnConnectionShutdown;
            _connection.ConnectionBlocked -= ConnectionOnConnectionBlocked;
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