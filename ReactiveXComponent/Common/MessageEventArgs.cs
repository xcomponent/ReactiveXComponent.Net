using Newtonsoft.Json.Linq;
using ReactiveXComponent.Common;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponent.Common
{
    using System;

    public class MessageEventArgs : EventArgs
    {        
        public MessageEventArgs(StateMachineRefHeader stateMachineRefHeader, object messageReceived)
        {
            StateMachineRefHeader = stateMachineRefHeader;
            MessageReceived = messageReceived;
        }

        public  StateMachineRefHeader StateMachineRefHeader { get; }

        public object MessageReceived { get; }

        public T GetMessage<T>(SerializationType serializationType) where T : class
        {
            if (serializationType == SerializationType.Binary)
            {
                return MessageReceived as T;
            }

            if (serializationType == SerializationType.Json
                     || serializationType == SerializationType.Bson)
            {
                var jResult = MessageReceived as JObject;
                return jResult?.ToObject<T>();
            }

            return null;
        }
    }
}
