using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMq
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(StateMachineRefHeader stateMachineRefHeader, object messageReceived)
        {
            StateMachineRefHeader = stateMachineRefHeader;
            MessageReceived = messageReceived;            
        }

        public  StateMachineRefHeader StateMachineRefHeader { get; }

        public object MessageReceived { get; }
    }
}
