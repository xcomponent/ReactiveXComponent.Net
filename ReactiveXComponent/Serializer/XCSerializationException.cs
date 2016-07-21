using System;

namespace ReactiveXComponent.Serializer
{
    [Serializable]
    public class XCSerializationException : Exception
    {
        public XCSerializationException()
        {
        }

        public XCSerializationException(string message) : base(message)
        {
        }

        public XCSerializationException(string message, Exception e) : base(message, e)
        {
        }
    }
}