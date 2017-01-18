namespace XCClientLib.RabbitMQ
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(Header header, object messageReceived)
        {
            this.Header = header;
            this.MessageReceived = messageReceived;            
        }

        public Header Header { get; protected set; }

        public object MessageReceived { get; protected set; }
    }
}
