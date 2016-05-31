using System.Text;

namespace ReactiveXComponent.RabbitMQ
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class RabbitMqHeaderConverter
    {
        public static Dictionary<string, object> ConvertHeader(Header header)
        {
            const int DefaultValue = -1;
            long[] ArrayDefaultValue = {-1};
            var encoding = new UnicodeEncoding();

            var dico = new Dictionary<string, object>
                           {
                               {HeaderElement.StateCode, DefaultValue},
                               {HeaderElement.StateMachineId, DefaultValue},
                               {HeaderElement.StateMachineCode, header.StateMachineCode != null ? header.StateMachineCode : DefaultValue},
                               {HeaderElement.ComponentCode, header.ComponentCode != null ? header.ComponentCode : DefaultValue},
                               {HeaderElement.EngineCode, header.EngineCode != null ? header.EngineCode : DefaultValue},
                               {HeaderElement.EventType, header.EventCode},     
                               {HeaderElement.Probes, ArrayDefaultValue },
                               {HeaderElement.MessageHashCode, DefaultValue},
                               {HeaderElement.IsContainsHashCode, false},
                               {HeaderElement.IncomingEventType, 0},
                               {HeaderElement.AgentId, DefaultValue },
                               {HeaderElement.PublishTopic, header.PublishTopic != null ? encoding.GetBytes(header.PublishTopic) : encoding.GetBytes(string.Empty) },
                               {HeaderElement.MessageType, header.MessageType != null ? encoding.GetBytes(header.MessageType) : encoding.GetBytes(string.Empty)}
                           };
            return dico;
        }

        public static Header ConvertHeader(IDictionary<string,object> header)
        {
            var encoding = new UnicodeEncoding();
            var stateCode = -1;
            long stateMachineId = -1;
            var stateMachineCode = -1;
            var componentCode = -1;
            var engineCode = -1;
            var eventType = -1;           
            var messageHashCode = -1;
            var isContainsHashCode = false;
            var incomingEventType = -1;
            var agentId = -1;
            var publishTopic = string.Empty;
            var messageType = string.Empty;

            if (header.ContainsKey(HeaderElement.StateMachineId))
                stateMachineId = Convert.ToInt64(header[HeaderElement.StateMachineId]);
            if (header.ContainsKey(HeaderElement.StateMachineCode))
                stateMachineCode = Convert.ToInt32(header[HeaderElement.StateMachineCode]);
            if (header.ContainsKey(HeaderElement.ComponentCode))
                componentCode = Convert.ToInt32(header[HeaderElement.ComponentCode]);
            if (header.ContainsKey(HeaderElement.EngineCode))
                engineCode = Convert.ToInt32(header[HeaderElement.EngineCode]);
            if (header.ContainsKey(HeaderElement.EventType))
                eventType = Convert.ToInt32(header[HeaderElement.EventType]);
            if (header.ContainsKey(HeaderElement.MessageHashCode))
                messageHashCode = Convert.ToInt32(header[HeaderElement.MessageHashCode]);
            if (header.ContainsKey(HeaderElement.IsContainsHashCode))
                isContainsHashCode = Convert.ToBoolean(header[HeaderElement.IsContainsHashCode]);
            if (header.ContainsKey(HeaderElement.StateCode))
                stateCode = Convert.ToInt32(header[HeaderElement.StateCode]);
            if (header.ContainsKey(HeaderElement.IncomingEventType))
                incomingEventType = Convert.ToInt32(header[HeaderElement.IncomingEventType]);
            if (header.ContainsKey(HeaderElement.AgentId))
                agentId = Convert.ToInt32(header[HeaderElement.AgentId]);
            if (header.ContainsKey(HeaderElement.PublishTopic))
                publishTopic = encoding.GetString(header[HeaderElement.PublishTopic] as byte[]);
            if (header.ContainsKey(HeaderElement.MessageType))
                messageType = encoding.GetString(header[HeaderElement.MessageType] as byte[]);

            return new Header
            {
                StateMachineCode = stateMachineCode,
                ComponentCode = componentCode,
                EngineCode = engineCode ,
                EventCode = eventType,
                MessageType  = messageType,
                PublishTopic = publishTopic
            };                
        }
    }
}
