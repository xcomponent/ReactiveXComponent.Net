using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Connection
{
    public class XCPublisher : IXCPublisher
    {
        public static string PrivateCommunicationIdentifier;
        private readonly XCConfiguration _configuration;

        public XCPublisher(XCConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEvent(string component, string stateMachine, object message, Visibility visibility)
        {
            var messageType = message?.GetType().ToString();
            var header = new Header()
            {
                StateMachineCode = _configuration.GetStateMachineCode(component, stateMachine),
                ComponentCode = _configuration.GetComponentCode(component),
                MessageType = messageType,
                EventCode = _configuration.GetPublisherEventCode(messageType),
                PublishTopic = visibility == Visibility.Private? PrivateCommunicationIdentifier : string.Empty
            };
        }
    }
}