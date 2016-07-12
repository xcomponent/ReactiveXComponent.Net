using System;
using System.IO;
using RabbitMQ.Client;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqPublisher : IXCPublisher
    {
        private bool _disposed;
        private readonly IXCConfiguration _configuration;
        private SatetMachineRef _satetMachineRef;
        private IModel _publisherChannel;
        private readonly string _exchangeName;
        private readonly string _component;
        private readonly string _privateCommunicationIdentifier;

        private readonly SerializerFactory _serializerFactory;

        public RabbitMqPublisher(string component, IXCConfiguration configuration, IConnection connection, SerializerFactory serializerFactory, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _exchangeName = configuration?.GetComponentCode(component).ToString();
            _configuration = configuration;
            CreatePublisherChannel(connection);
            _serializerFactory = serializerFactory;
        }

        private void CreatePublisherChannel(IConnection connection)
        {
            if (connection == null || !connection.IsOpen) return;
            _publisherChannel = connection.CreateModel();
            _publisherChannel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
        }

        public void SendEvent(string stateMachine, object message, Visibility visibility)
        {
            InitHeader(_component, stateMachine, message, visibility);

            var routingKey = _configuration.GetPublisherTopic(_component, stateMachine, (int)_satetMachineRef?.EventCode);
            var prop = _publisherChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.ConvertHeader(_satetMachineRef);
            message = message ?? 0;

            Send(message, routingKey, prop);
        }

        private void InitHeader(string component, string stateMachine, object message, Visibility visibility)
        {
            var messageType = message?.GetType().ToString() ?? string.Empty;
            try
            {
                _satetMachineRef = new SatetMachineRef
                {
                    StateMachineCode = _configuration.GetStateMachineCode(component, stateMachine),
                    ComponentCode = _configuration.GetComponentCode(component),
                    MessageType = messageType,
                    EventCode = _configuration.GetPublisherEventCode(messageType),
                    PublishTopic = visibility == Visibility.Private ? _privateCommunicationIdentifier : string.Empty
                };
            }
            catch (NullReferenceException e)
            {
                throw new NullReferenceException("Failed to init SatetMachineRef", e);  
            }
        }

        private void Send(object message, string routingKey, IBasicProperties properties)
        {
            var serializer = _serializerFactory.CreateSerializer();
            byte[] messageBytes;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, message);
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