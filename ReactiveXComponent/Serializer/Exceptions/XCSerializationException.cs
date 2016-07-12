using System;

namespace XComponent.Communication.Serialization.Exceptions
{
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