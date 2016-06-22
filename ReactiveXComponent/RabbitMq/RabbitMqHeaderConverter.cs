using System.Text;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMq
{
    using System;
    using System.Collections.Generic;

    public static class RabbitMqHeaderConverter
    {
        public static Dictionary<string, object> ConvertHeader(Header header)
        {
            const int defaultValue = -1;
            long[] arrayDefaultValue = {-1};
            var encoding = new UnicodeEncoding();

            var dico = new Dictionary<string, object>
                           {
                               {HeaderElement.StateCode, defaultValue},
                               {HeaderElement.StateMachineId, defaultValue},
                               {HeaderElement.StateMachineCode, header?.StateMachineCode ?? defaultValue},
                               {HeaderElement.ComponentCode, header?.ComponentCode ?? defaultValue},
                               {HeaderElement.EventType, header?.EventCode ?? defaultValue},     
                               {HeaderElement.Probes, arrayDefaultValue },
                               {HeaderElement.MessageHashCode, defaultValue},
                               {HeaderElement.IsContainsHashCode, false},
                               {HeaderElement.IncomingEventType, 0},
                               {HeaderElement.AgentId, defaultValue },
                               {HeaderElement.PublishTopic, header?.PublishTopic != null ? encoding.GetBytes(header.PublishTopic) : encoding.GetBytes(string.Empty) },
                               {HeaderElement.MessageType, header?.MessageType != null ? encoding.GetBytes(header.MessageType) : encoding.GetBytes(string.Empty)}
                           };
            return dico;
        }

        public static Header ConvertHeader(IDictionary<string,object> header)
        {
            var encoding = new UnicodeEncoding();
            var stateMachineCode = -1;
            var componentCode = -1;
            var eventType = -1;           
            var publishTopic = String.Empty;
            var messageType = String.Empty;

            if (header.ContainsKey(HeaderElement.StateMachineCode))
                stateMachineCode = Convert.ToInt32(header[HeaderElement.StateMachineCode]);
            if (header.ContainsKey(HeaderElement.ComponentCode))
                componentCode = Convert.ToInt32(header[HeaderElement.ComponentCode]);
            if (header.ContainsKey(HeaderElement.EventType))
                eventType = Convert.ToInt32(header[HeaderElement.EventType]);
            if (header.ContainsKey(HeaderElement.PublishTopic))
                publishTopic = encoding.GetString(header[HeaderElement.PublishTopic] as byte[]);
            if (header.ContainsKey(HeaderElement.MessageType))
                messageType = encoding.GetString(header[HeaderElement.MessageType] as byte[]);

            return new Header
            {
                StateMachineCode = stateMachineCode,
                ComponentCode = componentCode,
                EventCode = eventType,
                MessageType  = messageType,
                PublishTopic = publishTopic
            };                
        }
    }
}
