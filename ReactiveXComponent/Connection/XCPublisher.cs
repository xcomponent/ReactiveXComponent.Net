using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent.Connection
{
    public class XCPublisher : IXCPublisher
    {
        public static string PrivateCommunicationIdentifier;
        private readonly DeploymentParser _parser;

        public DeploymentParser Parser => _parser;

        public XCPublisher(DeploymentParser parser)
        {
            _parser = parser;
        }

        public void SendEvent(string component, string stateMachine, object message, Visibility visibility)
        {
            var messageType = message?.GetType().ToString();
            var header = new Header()
            {
                StateMachineCode = _parser.GetStateMachineCode(component, stateMachine),
                ComponentCode = _parser.GetComponentCode(component),
                MessageType = messageType,
                EventCode = _parser.GetPublisherEventCode(messageType),
                PublishTopic = visibility == Visibility.Private? PrivateCommunicationIdentifier : string.Empty
            };
            //Send to RabbitMQ
            //_publisher.Send(header, message, _parser.GetPublisherTopic(component, stateMachine, eventCode));
        }
    }
}