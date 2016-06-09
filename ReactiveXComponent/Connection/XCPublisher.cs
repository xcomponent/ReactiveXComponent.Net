using System;
using System.IO;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Connection
{
    public class XCPublisher : IXCPublisher
    {
        private bool _disposed;
        private string _privateCommunicationIdentifier;
        //private readonly string _component;
        //private readonly DeploymentParser _parser

        public XCPublisher(string component, Stream xcApiStream)
        {
            //_component = component;
            //_parser = new DeploymentParser(xcApiStream);
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
            //Send to RabbitMQ
            //_publisher.Send(header, message, _parser.GetPublisherTopic(component, stateMachine, eventCode));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                
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