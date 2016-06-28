using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMQ
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(Header header, object messageReceived)
        {
            Header = header;
            MessageReceived = messageReceived;            
        }

        public Header Header { get; }

        public object MessageReceived { get; }
    }
}
