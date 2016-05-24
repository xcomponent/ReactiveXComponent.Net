using System.Text;

namespace XCClientLib.RabbitMQ
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class RabbitMQHeaderConverter
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
                               {HeaderElement.MessageType, header.MessageType != null ? encoding.GetBytes(header.MessageType) : encoding.GetBytes(string.Empty)},
                              //{HeaderElement.SessionData, header.SessionData != null ? encoding.GetBytes(header.SessionData.Value) : encoding.GetBytes(string.Empty)}
                           };
            return dico;
        }

        public static Header ConvertHeader(IDictionary<string,object> header)
        {
            var encoding = new UnicodeEncoding();
            var stateCode = -1; // int
            long stateMachineId = -1; //long
            int stateMachineCode = -1; //int
            int componentCode = -1; //int
            int engineCode = -1; //int
            int eventType = -1; //int
            long[] probes = {-1}; //array            
            int messageHashCode = -1; //int
            bool isContainsHashCode = false; //bool
            var incomingEventType = -1; // int
            var agentId = -1; // int
            var publishTopic = encoding.GetBytes(string.Empty); //string
            var messageType = encoding.GetBytes(string.Empty); // string
            //var sessionData = encoding.GetBytes(string.Empty); //string 

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
            {
                stateCode = Convert.ToInt32(header[HeaderElement.StateCode]);
            }

            if (header.ContainsKey(HeaderElement.IncomingEventType))
            {
                incomingEventType = Convert.ToInt32(header[HeaderElement.IncomingEventType]);
            }

            if (header.ContainsKey(HeaderElement.AgentId))
            {
                agentId = Convert.ToInt32(header[HeaderElement.AgentId]);
            }
            if (header.ContainsKey(HeaderElement.PublishTopic))
                publishTopic = encoding.GetBytes(header[HeaderElement.PublishTopic].ToString());
            if (header.ContainsKey(HeaderElement.MessageType))
                messageType = encoding.GetBytes(header[HeaderElement.MessageType].ToString());
            //if (header.ContainsKey(HeaderElement.SessionData))
            //    sessionData = Convert.ToString(header[HeaderElement.SessionData]);
            

            return new Header { ComponentCode = componentCode,
                StateMachineCode = stateMachineCode,
                EngineCode = engineCode ,
                PublishTopic = publishTopic.ToString(),
                MessageType  = messageType.ToString()
            };                
        }
    }
}
