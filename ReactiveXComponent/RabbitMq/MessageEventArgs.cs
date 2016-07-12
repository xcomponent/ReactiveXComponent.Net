using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMQ
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(SatetMachineRef satetMachineRef, object messageReceived)
        {
            SatetMachineRef = satetMachineRef;
            MessageReceived = messageReceived;            
        }

        public SatetMachineRef SatetMachineRef { get; }

        public object MessageReceived { get; }
    }
}
