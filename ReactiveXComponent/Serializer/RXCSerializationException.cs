using System;

namespace ReactiveXComponent.Serializer
{
    [Serializable]
    public class RXCSerializationException : Exception
    {
        public RXCSerializationException()
        {
        }

        public RXCSerializationException(string message) : base(message)
        {
        }

        public RXCSerializationException(string message, Exception e) : base(message, e)
        {
        }
    }
}