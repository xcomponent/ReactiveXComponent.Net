using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMq
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(StateMachineRef stateMachineRef, object messageReceived)
        {
            StateMachineRef = stateMachineRef;
            MessageReceived = messageReceived;            
        }

        public StateMachineRef StateMachineRef { get; }

        public object MessageReceived { get; }
    }
}
