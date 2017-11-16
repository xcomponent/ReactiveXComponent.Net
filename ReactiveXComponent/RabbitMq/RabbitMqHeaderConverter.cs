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
            var encoding = new UTF8Encoding();

            var dico = new Dictionary<string, object>
                           {
                               {HeaderElement.StateMachineCode, header?.StateMachineCode},
                               {HeaderElement.ComponentCode, header?.ComponentCode},
                               {HeaderElement.StateCode, header?.StateCode},
                               {HeaderElement.EventType, header?.EventCode},
                               {HeaderElement.IncomingEventType, header?.IncomingEventType},
                               {HeaderElement.PublishTopic, header?.PublishTopic != null ? encoding.GetBytes(header.PublishTopic) : encoding.GetBytes(string.Empty) },
                               {HeaderElement.MessageType, header?.MessageType != null ? encoding.GetBytes(header.MessageType) : encoding.GetBytes(string.Empty)},
                               {HeaderElement.ErrorMessage, header?.ErrorMessage != null ? encoding.GetBytes(header.ErrorMessage) : encoding.GetBytes(string.Empty)}
                           };
            return dico;
        }

        public static Dictionary<string, object> CreateHeaderFromStateMachineRefHeader(StateMachineRefHeader stateMachineRefHeader, IncomingEventType incomingEventType, int eventCode)
        {
            var encoding = new UTF8Encoding();

            var dico = new Dictionary<string, object>
                           {
                                {HeaderElement.StateMachineId, stateMachineRefHeader?.StateMachineId},
                                {HeaderElement.StateCode, stateMachineRefHeader?.StateCode},
                                {HeaderElement.StateMachineCode, stateMachineRefHeader?.StateMachineCode},
                                {HeaderElement.ComponentCode, stateMachineRefHeader?.ComponentCode},
                                {HeaderElement.PublishTopic, stateMachineRefHeader?.PrivateTopic != null ? encoding.GetBytes(stateMachineRefHeader.PrivateTopic) : encoding.GetBytes(string.Empty) },
                                {HeaderElement.MessageType, stateMachineRefHeader?.MessageType != null ? encoding.GetBytes(stateMachineRefHeader.MessageType) : encoding.GetBytes(string.Empty)},
                                {HeaderElement.EventType, eventCode},
                                {HeaderElement.IncomingEventType, (int)incomingEventType},
                                {HeaderElement.ErrorMessage, stateMachineRefHeader?.ErrorMessage != null ? encoding.GetBytes(stateMachineRefHeader.ErrorMessage) : encoding.GetBytes(string.Empty)}
                           };
            return dico;
        }

        public static StateMachineRefHeader ConvertStateMachineRefHeader(IDictionary<string,object> stateMachineRefHeader)
        {
            var encoding = new UTF8Encoding();
            var stateMachineId = -1;
            var stateCode = -1;
            var stateMachineCode = -1;
            var componentCode = -1;     
            var publishTopic = string.Empty;
            var messageType = string.Empty;
            var sessionData = string.Empty;
            var errorMessage = string.Empty;

            if (stateMachineRefHeader.ContainsKey(HeaderElement.StateMachineId))
                stateMachineId = Convert.ToInt32(stateMachineRefHeader[HeaderElement.StateMachineId]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.StateCode))
                stateCode = Convert.ToInt32(stateMachineRefHeader[HeaderElement.StateCode]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.StateMachineCode))
                stateMachineCode = Convert.ToInt32(stateMachineRefHeader[HeaderElement.StateMachineCode]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.ComponentCode))
                componentCode = Convert.ToInt32(stateMachineRefHeader[HeaderElement.ComponentCode]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.PublishTopic))
                publishTopic = encoding.GetString(stateMachineRefHeader[HeaderElement.PublishTopic] as byte[]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.MessageType))
                messageType = encoding.GetString(stateMachineRefHeader[HeaderElement.MessageType] as byte[]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.SessionData))
                sessionData = encoding.GetString(stateMachineRefHeader[HeaderElement.SessionData] as byte[]);
            if (stateMachineRefHeader.ContainsKey(HeaderElement.ErrorMessage))
                errorMessage = encoding.GetString(stateMachineRefHeader[HeaderElement.ErrorMessage] as byte[]);

            return new StateMachineRefHeader()
            {
                StateMachineId = stateMachineId,
                StateCode = stateCode,
                StateMachineCode = stateMachineCode,
                ComponentCode = componentCode,
                MessageType  = messageType,
                PrivateTopic = publishTopic,
                SessionData = sessionData,
                ErrorMessage = errorMessage
            };                
        }
    }
}
