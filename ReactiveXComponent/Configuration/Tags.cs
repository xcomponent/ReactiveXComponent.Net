using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.Configuration
{
    public sealed class Tags
    {
        public Tags()
        {
            Bus = "bus";
            Websocket = "websocket";
            Host = "host";
            User = "user";
            Password = "password";
            Port = "port";
            Name = "name";
            Id = "id";
            Publish = "publish";
            EventName = "event";
            EventCode = "eventCode";
            ComponentCode = "componentCode";
            StateMachineCode = "stateMachineCode";
            Topic = "type";
            Subscribe = "subscribe";
            EventType = "eventType";
            Update = "Update";
            TopicType = "topicType";
            Input = "input";
            Output = "output";
        }


        public string Input { get; private set; }

        public string Output { get; private set; }

        public string TopicType { get; private set; }

        public string Update { get; private set; }

        public string EventType { get; private set; }

        public string Subscribe { get; private set; }

        public string Topic { get; private set; }

        public string StateMachineCode { get; private set; }

        public string ComponentCode { get; private set; }

        public string EventCode { get; private set; }

        public string EventName { get; private set; }

        public string Publish { get; private set; }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Port { get; private set; }

        public string Password { get; private set; }

        public string User { get; private set; }

        public string Host { get; private set; }

        public string Websocket { get; private set; }

        public string Bus { get; private set; }
    }
}
