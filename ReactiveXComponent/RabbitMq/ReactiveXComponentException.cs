using System;
using System.Runtime.Serialization;

namespace ReactiveXComponent.RabbitMq
{
    [Serializable]
    public class ReactiveXComponentException : Exception
    {
        public ReactiveXComponentException() { }
        public ReactiveXComponentException(string message) : base(message) { }
        public ReactiveXComponentException(string message, Exception inner) : base(message, inner) { }

        protected ReactiveXComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

