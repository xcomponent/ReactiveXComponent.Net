using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RabbitMQ.Client;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqPublisher : IXCPublisher
    {
        private bool _disposed;
        private readonly IXCConfiguration _configuration;
        private Header _header;
        private IModel _publisherChannel;
        private readonly string _exchangeName;
        private readonly string _component;

        public IModel PublisherChanne => _publisherChannel;

        public RabbitMqPublisher(string component, IXCConfiguration configuration, IConnection connection)
        {
            _component = component;
            _exchangeName = configuration?.GetComponentCode(component).ToString();
            _configuration = configuration;
            CreatePublisherChannel(connection);
        }

        private void CreatePublisherChannel(IConnection connection)
        {
            if (connection != null && connection.IsOpen)
            {
                _publisherChannel = connection.CreateModel();
                _publisherChannel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
            }
        }

        public void SendEvent(string stateMachine, object message, Visibility visibility)
        {
            InitHeader(_component, stateMachine, message, visibility);

            var routingKey = _configuration.GetPublisherTopic(_component, stateMachine, (int)_header?.EventCode);
            var prop = _publisherChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.ConvertHeader(_header);
            message = message ?? 0;

            Send(message, routingKey, prop);
        }

        private void InitHeader(string component, string stateMachine, object message, Visibility visibility)
        {
            var messageType = message?.GetType().ToString() ?? string.Empty;
            try
            {
                _header = new Header
                {
                    StateMachineCode = _configuration.GetStateMachineCode(component, stateMachine),
                    ComponentCode = _configuration.GetComponentCode(component),
                    MessageType = messageType,
                    EventCode = _configuration.GetPublisherEventCode(messageType),
                    PublishTopic =
                    visibility == Visibility.Private ? XComponentApi.PrivateCommunicationIdentifier : string.Empty
                };
            }
            catch (NullReferenceException e)
            {
                throw new NullReferenceException("Failed to init Header", e);  
            }
        }

        private void Send(object message, string routingKey, IBasicProperties properties)
        {
            byte[] messageBytes;
            using (var stream = new MemoryStream())
            {
                Serialize(stream, message);
                messageBytes = stream.ToArray();
            }

            if (messageBytes == null)
                throw new Exception("Message serialisation failed");

            try
            {
                _publisherChannel.BasicPublish(_exchangeName, routingKey, properties, messageBytes);
            }
            catch (Exception exception)
            {
                throw new Exception("The publication failed: " + exception.Message, exception);
            }
        }

        private void Serialize(Stream stream, object message)
        {
            if (message != null)
            {
                var binaryFormater = new BinaryFormatter();
                binaryFormater.Serialize(stream, message);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _publisherChannel?.Close();
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