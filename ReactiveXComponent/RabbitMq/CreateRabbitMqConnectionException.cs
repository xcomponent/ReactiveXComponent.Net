using System;
using System.Runtime.Serialization;

namespace ReactiveXComponent.RabbitMq
{
    [Serializable]
    public class CreateRabbitMqConnectionException : Exception
    {
        public CreateRabbitMqConnectionException() { }
        public CreateRabbitMqConnectionException(string message) : base(message) { }
        public CreateRabbitMqConnectionException(string message, Exception inner) : base(message, inner) { }

        protected CreateRabbitMqConnectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}