using System.Text;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.RabbitMq
{
    using System;
    using System.Collections.Generic;

    public static class RabbitMqHeaderConverter
    {
        public static Dictionary<string, object> ConvertHeader(Header satetMachineRef)
        {
            const int defaultValue = -1;
            var encoding = new UnicodeEncoding();

            var dico = new Dictionary<string, object>
                           {
                               {HeaderElement.StateMachineCode, satetMachineRef?.StateMachineCode ?? defaultValue},
                               {HeaderElement.ComponentCode, satetMachineRef?.ComponentCode ?? defaultValue},
                               {HeaderElement.EventType, satetMachineRef?.EventCode ?? defaultValue},     
                               {HeaderElement.PublishTopic, satetMachineRef?.PublishTopic != null ? encoding.GetBytes(satetMachineRef.PublishTopic) : encoding.GetBytes(string.Empty) },
                               {HeaderElement.MessageType, satetMachineRef?.MessageType != null ? encoding.GetBytes(satetMachineRef.MessageType) : encoding.GetBytes(string.Empty)}
                           };
            return dico;
        }

        public static StateMachineRef ConvertStateMachineRef(IDictionary<string,object> stateMachineRef)
        {
            var encoding = new UnicodeEncoding();
            var stateMachineId = -1;
            var agentId = -1;
            var stateMachineCode = -1;
            var componentCode = -1;
            var eventType = -1;           
            var publishTopic = String.Empty;
            var messageType = String.Empty;

            if (stateMachineRef.ContainsKey(HeaderElement.StateMachineId))
                stateMachineId = Convert.ToInt32(stateMachineRef[HeaderElement.StateMachineId]);
            if (stateMachineRef.ContainsKey(HeaderElement.AgentId))
                agentId = Convert.ToInt32(stateMachineRef[HeaderElement.AgentId]);
            if (stateMachineRef.ContainsKey(HeaderElement.StateMachineCode))
                stateMachineCode = Convert.ToInt32(stateMachineRef[HeaderElement.StateMachineCode]);
            if (stateMachineRef.ContainsKey(HeaderElement.ComponentCode))
                componentCode = Convert.ToInt32(stateMachineRef[HeaderElement.ComponentCode]);
            if (stateMachineRef.ContainsKey(HeaderElement.EventType))
                eventType = Convert.ToInt32(stateMachineRef[HeaderElement.EventType]);
            if (stateMachineRef.ContainsKey(HeaderElement.PublishTopic))
                publishTopic = encoding.GetString(stateMachineRef[HeaderElement.PublishTopic] as byte[]);
            if (stateMachineRef.ContainsKey(HeaderElement.MessageType))
                messageType = encoding.GetString(stateMachineRef[HeaderElement.MessageType] as byte[]);

            return new StateMachineRef()
            {
                StateMachineId = stateMachineId,
                AgentId = agentId,
                StateMachineCode = stateMachineCode,
                ComponentCode = componentCode,
                EventCode = eventType,
                MessageType  = messageType,
                PublishTopic = publishTopic
            };                
        }
    }
}
