using System;
using System.Runtime.Serialization;

namespace ReactiveXComponent.Configuration
{
    [Serializable]
    public class InitConfigurationException : Exception
    {
        public InitConfigurationException() { }
        public InitConfigurationException (string message) : base(message) { }
        public InitConfigurationException(string message, Exception inner) : base(message, inner) { }

        protected InitConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}