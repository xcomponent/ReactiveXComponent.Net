using System;
using System.Runtime.Serialization;

namespace ReactiveXComponent.RabbitMq
{
    [Serializable]
    public class XComponentException : Exception
    {
        public XComponentException() { }
        public XComponentException(string message) : base(message) { }
        public XComponentException(string message, Exception inner) : base(message, inner) { }

        protected XComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

