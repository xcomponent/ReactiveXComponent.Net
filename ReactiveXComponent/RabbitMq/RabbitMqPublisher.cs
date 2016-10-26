using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using RabbitMQ.Client;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqPublisher : IXCPublisher
    {
        private readonly IXCConfiguration _configuration;
        private IModel _publisherChannel;
        private readonly string _exchangeName;
        private readonly string _component;
        private readonly string _privateCommunicationIdentifier;
        private readonly ISerializer _serializer;
        private readonly RabbitMqSnapshot _rabbitMqSnapshot;

        public RabbitMqPublisher(string component, IXCConfiguration configuration, IConnection connection, ISerializer serializer, string privateCommunicationIdentifier = null)
        {
            _component = component;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _exchangeName = configuration?.GetComponentCode(component).ToString();
            _configuration = configuration;
            CreatePublisherChannel(connection);
            _serializer = serializer;
            _rabbitMqSnapshot = new RabbitMqSnapshot(connection, component, configuration, _serializer, privateCommunicationIdentifier);
        }

        private void CreatePublisherChannel(IConnection connection)
        {
            if (connection == null || !connection.IsOpen)
            {
                return;
            }

            _publisherChannel = connection.CreateModel();
            _publisherChannel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
        }

        public void SendEvent(string stateMachine, object message, Visibility visibility = Visibility.Public)
        {
            var header = CreateHeader(_component, stateMachine, message, visibility);

            if (header == null) return;

            string routingKey = _configuration.GetPublisherTopic(_component, stateMachine);

            var prop = _publisherChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.ConvertHeader(header);
            message = message ?? 0;

            Send(message, routingKey, prop);
        }

        public void SendEvent(StateMachineRefHeader stateMachineRefHeader, object message, Visibility visibility = Visibility.Public)
        {
            if (stateMachineRefHeader == null) return;

            var stateMachineRefNewHeader = CreateStateMachineRefHeader(stateMachineRefHeader, message);

            string routingKey = _configuration.GetPublisherTopic(_component, stateMachineRefHeader.StateMachineCode.ToString());

            var prop = _publisherChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.CreateHeaderFromStateMachineRefHeader(stateMachineRefNewHeader, IncomingEventType.Transition);
            message = message?? 0;

            Send(message, routingKey, prop);
        }

        public List<MessageEventArgs> GetSnapshot(string stateMachine, int timeout)
        {
            _rabbitMqSnapshot.GetSnapshot(stateMachine, null, timeout);
            return _rabbitMqSnapshot.StateMachineInstances;
        }

        public void GetSnapshotAsync(string stateMachine, Action<List<MessageEventArgs>> onSnapshotReceived)
        {
            _rabbitMqSnapshot.GetSnapshot(stateMachine, onSnapshotReceived);
        }

        private Header CreateHeader(string component, string stateMachine, object message, Visibility visibility)
        {
            var messageType = message?.GetType().ToString() ?? string.Empty;

            if (_configuration == null)
            {
                return null;
            }

            var header = new Header
            {
                StateMachineCode = _configuration.GetStateMachineCode(component, stateMachine),
                ComponentCode = _configuration.GetComponentCode(component),
                MessageType = messageType,
                EventCode = _configuration.GetPublisherEventCode(messageType),
                IncomingEventType = (int)IncomingEventType.Transition,
                PublishTopic = visibility == Visibility.Private ? _privateCommunicationIdentifier : string.Empty                
            };

            return header;
        }

        private StateMachineRefHeader CreateStateMachineRefHeader(StateMachineRefHeader stateMachineRefHeader, object message)
        {
            var messageType = message?.GetType().ToString() ?? string.Empty;

            if (_configuration == null)
            {
                return null;
            }

            var stateMachineRefheader = new StateMachineRefHeader()
            {
                StateMachineId = stateMachineRefHeader.StateMachineId,
                AgentId = stateMachineRefHeader.AgentId,
                StateCode = stateMachineRefHeader.StateCode,
                StateMachineCode = stateMachineRefHeader.StateMachineCode,
                ComponentCode = stateMachineRefHeader.ComponentCode,
                MessageType = messageType,
                EventCode = _configuration.GetPublisherEventCode(messageType),
                PublishTopic = stateMachineRefHeader.PublishTopic
            };

            return stateMachineRefheader;
        }

        private void Send(object message, string routingKey, IBasicProperties properties)
        {
            if (message == null)
            {
                throw new ReactiveXComponentException("Given message is null");
            }

            byte[] messageBytes;
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(stream, message);
                messageBytes = stream.ToArray();
            }

            try
            {
                _publisherChannel.BasicPublish(_exchangeName, routingKey, properties, messageBytes);
            }
            catch (Exception exception)
            {
                throw new ReactiveXComponentException("The publication failed: " + exception.Message, exception);
            }
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // clear managed resources
                    _publisherChannel?.Dispose();
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

        ~RabbitMqPublisher()
        {
            Dispose(false);
        }

        #endregion

    }
}