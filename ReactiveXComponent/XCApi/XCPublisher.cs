using System;
using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public class XCPublisher : IXCPublisher
    {
        private bool _disposed = false;

        private readonly IRabbitMqPublisherFactory _publisherFactory;
        private readonly DeploymentParser _parser;

        private IRabbitMqPublisher _publisher;
        private string _privateCommunicationIdentifier;

        public XCPublisher(IRabbitMqPublisherFactory publisherFactory, DeploymentParser parser)
        {
            _publisherFactory = publisherFactory;
            _parser = parser;
        }

        public void CreatePublisher(string component)
        {
            _publisher = _publisherFactory.Create(component);
        }

        public void InitPrivateCommunication(string privateCommunicationIdentifier)
        {
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public void SendEvent(string engine, string component, string stateMachine, int eventCode, string messageType, object message, Visibility visibility)
        {
            var header = new Header()
            {
                ComponentCode = HashcodeHelper.GetXcHashCode(component),
                EngineCode = HashcodeHelper.GetXcHashCode(engine),
                StateMachineCode = HashcodeHelper.GetXcHashCode(stateMachine),
                MessageType = messageType,
                EventCode = eventCode,
                PublishTopic = string.Empty
            };

            if (visibility == Visibility.Private)
            {
                header.PublishTopic = _privateCommunicationIdentifier;
            }
            _publisher.Send(header, message, _parser.GetPublisherTopic(component, stateMachine, eventCode));
        }

        public void Close()
        {
            _publisher?.Close();
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
                return;
            else
            {
                Close();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(_disposed);
            GC.SuppressFinalize(this);
        }
    }
}
