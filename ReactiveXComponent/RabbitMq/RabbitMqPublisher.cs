using System;
using System.Threading;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqPublisher : IXCPublisher
    {
        private readonly XCConfiguration _configuration;
        private Header _header;

        public RabbitMqPublisher(XCConfiguration configuration)
        {
            _configuration = configuration;
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

        public void SendEvent(string component, string stateMachine, object message, Visibility visibility)
        {
           InitHeader(component, stateMachine, message, visibility);
        }
    }
}