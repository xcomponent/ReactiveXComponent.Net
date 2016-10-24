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
            var encoding = new UTF8Encoding();

            var dico = new Dictionary<string, object>
                           {
                               {HeaderElement.StateMachineCode, header?.StateMachineCode ?? defaultValue},
                               {HeaderElement.ComponentCode, header?.ComponentCode ?? defaultValue},
                               {HeaderElement.EventType, header?.EventCode ?? defaultValue},
                               {HeaderElement.IncomingEventType, header?.IncomingEventType ?? defaultValue},
                               {HeaderElement.PublishTopic, header?.PublishTopic != null ? encoding.GetBytes(header.PublishTopic) : encoding.GetBytes(string.Empty) },
                               {HeaderElement.MessageType, header?.MessageType != null ? encoding.GetBytes(header.MessageType) : encoding.GetBytes(string.Empty)}
                           };
            return dico;
        }

        public static Dictionary<string, object> CreateHeaderFromStateMachineRefHeader(StateMachineRefHeader stateMachineRefHeader, IncomingEventType incomingEventType)
        {
            const int defaultValue = -1;
            var encoding = new UTF8Encoding();

            var dico = new Dictionary<string, object>
                           {
                                {HeaderElement.StateMachineId, stateMachineRefHeader?.StateMachineId ?? defaultValue},
                                {HeaderElement.AgentId, stateMachineRefHeader?.AgentId ?? defaultValue},
                                {HeaderElement.StateCode, stateMachineRefHeader?.StateCode ?? defaultValue},
                                {HeaderElement.StateMachineCode, stateMachineRefHeader?.StateMachineCode ?? defaultValue},
                                {HeaderElement.ComponentCode, stateMachineRefHeader?.ComponentCode ?? defaultValue},
                                {HeaderElement.EventType, stateMachineRefHeader?.EventCode ?? defaultValue},
                                {HeaderElement.PublishTopic, stateMachineRefHeader?.PublishTopic != null ? encoding.GetBytes(stateMachineRefHeader.PublishTopic) : encoding.GetBytes(string.Empty) },
                                {HeaderElement.MessageType, stateMachineRefHeader?.MessageType != null ? encoding.GetBytes(stateMachineRefHeader.MessageType) : encoding.GetBytes(string.Empty)},
                                {HeaderElement.IncomingEventType, (int)incomingEventType}
                           };
            return dico;
        }

        public static StateMachineRefHeader ConvertStateMachineRefHeader(IDictionary<string,object> stateMachineRefHeader)
        {
            var encoding = new UTF8Encoding();
            var stateMachineId = -1;
            var agentId = -1;
            var stateCode = -1;
            var stateMachineCode = -1;
            var componentCode = -1;
            var eventType = -1;           
            var publishTopic = String.Empty;
            var messageType = String.Empty;

            if (stateMachineRefHeader.ContainsKey(HeaderElement.StateMachineId))
                stateMachineId = Convert.ToInt32(stateMachineRefHeader[HeaderElement.StateMachineId]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.AgentId))
                agentId = Convert.ToInt32(stateMachineRefHeader[HeaderElement.AgentId]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.StateCode))
                stateCode = Convert.ToInt32(stateMachineRefHeader[HeaderElement.StateCode]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.StateMachineCode))
                stateMachineCode = Convert.ToInt32(stateMachineRefHeader[HeaderElement.StateMachineCode]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.ComponentCode))
                componentCode = Convert.ToInt32(stateMachineRefHeader[HeaderElement.ComponentCode]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.EventType))
                eventType = Convert.ToInt32(stateMachineRefHeader[HeaderElement.EventType]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.PublishTopic))
                publishTopic = encoding.GetString(stateMachineRefHeader[HeaderElement.PublishTopic] as byte[]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.MessageType))
                messageType = encoding.GetString(stateMachineRefHeader[HeaderElement.MessageType] as byte[]);

            return new StateMachineRefHeader()
            {
                StateMachineId = stateMachineId,
                AgentId = agentId,
                StateCode = stateCode,
                StateMachineCode = stateMachineCode,
                ComponentCode = componentCode,
                EventCode = eventType,
                MessageType  = messageType,
                PublishTopic = publishTopic
            };                
        }
    }
}
