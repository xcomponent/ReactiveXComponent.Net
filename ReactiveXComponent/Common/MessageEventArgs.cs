using Newtonsoft.Json.Linq;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.Common
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(StateMachineRefHeader stateMachineRefHeader, object messageReceived, SerializationType serializationType)
        {
            StateMachineRefHeader = stateMachineRefHeader;
            MessageReceived = messageReceived;
            SerializationType = serializationType;
        }

        public  StateMachineRefHeader StateMachineRefHeader { get; }

        public object MessageReceived { get; }

        public SerializationType SerializationType { get; }

        public T GetMessage<T>() where T : class
        {
            if (SerializationType == SerializationType.Binary)
            {
                return MessageReceived as T;
            }

            if (SerializationType == SerializationType.Json
                     || SerializationType == SerializationType.Bson)
            {
                var jResult = MessageReceived as JObject;
                return jResult?.ToObject<T>();
            }

            return null;
        }
    }
}
