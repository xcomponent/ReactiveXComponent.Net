﻿using System;
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
        private IModel _publisherChannel;
        private readonly string _exchangeName;
        private readonly string _component;
        private readonly string _privateCommunicationIdentifier;
         
        private readonly ISerializer _serializer;

        public RabbitMqPublisher(string component, IXCConfiguration configuration, IConnection connection, ISerializer serializer, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _exchangeName = configuration?.GetComponentCode(component).ToString();
            _configuration = configuration;
            CreatePublisherChannel(connection);
            _serializer = serializer;
        }

        private void CreatePublisherChannel(IConnection connection)
        {
            if (connection == null || !connection.IsOpen) return;
            _publisherChannel = connection.CreateModel();
            _publisherChannel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
        }

        public void SendEvent(string stateMachine, object message, Visibility visibility)
        {
            var header = CreateHeader(_component, stateMachine, message, visibility);
            if (header == null) return;
            var routingKey = !string.IsNullOrEmpty(_privateCommunicationIdentifier)? _privateCommunicationIdentifier:
                            _configuration.GetPublisherTopic(_component, stateMachine, header.EventCode);
            var prop = _publisherChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.ConvertHeader(header);
            message = message ?? 0;

            Send(message, routingKey, prop);
        }

        private Header CreateHeader(string component, string stateMachine, object message, Visibility visibility)
        {
            var messageType = message?.GetType().ToString() ?? string.Empty;
            if (_configuration == null) return null;
            var header = new Header
            {
                StateMachineCode = _configuration.GetStateMachineCode(component, stateMachine),
                ComponentCode = _configuration.GetComponentCode(component),
                MessageType = messageType,
                EventCode = _configuration.GetPublisherEventCode(messageType),
                PublishTopic = visibility == Visibility.Private ? _privateCommunicationIdentifier : string.Empty
            };
            return header;
        }

        private void Send(object message, string routingKey, IBasicProperties properties)
        {
            
            byte[] messageBytes;
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(stream, message);
                messageBytes = stream.ToArray();
            }

            if (messageBytes == null)
                throw new ReactiveXComponentException("Message serialisation failed");

            try
            {
                _publisherChannel.BasicPublish(_exchangeName, routingKey, properties, messageBytes);
            }
            catch (ReactiveXComponentException exception)
            {
                throw new ReactiveXComponentException("The publication failed: " + exception.Message, exception);
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
        }
    }
}