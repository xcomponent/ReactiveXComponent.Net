using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMq
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(StateMachineRefHeader stateMachineRefHeader, object messageReceived, string replyTopic = null)
        {
            StateMachineRefHeader = stateMachineRefHeader;
            MessageReceived = messageReceived;
            ReplyTopic = replyTopic;
        }

        public  StateMachineRefHeader StateMachineRefHeader { get; }

        public object MessageReceived { get; }

        public string ReplyTopic { get; }
    }
}
