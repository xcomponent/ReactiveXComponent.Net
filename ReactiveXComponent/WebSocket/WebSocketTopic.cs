using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketTopic
    {
        public string Key { get; set; }

        public WebSocketTopicKind Kind { get; set; }

        public WebSocketTopic(string key, WebSocketTopicKind kind)
        {
            Kind = kind;
            Key = key;
        }

        public static WebSocketTopic Snapshot(string topic)
        {
            return new WebSocketTopic(topic, WebSocketTopicKind.Snapshot);
        }

        public static WebSocketTopic Private(string topic)
        {
            return new WebSocketTopic(topic, WebSocketTopicKind.Private);
        }

        public static WebSocketTopic Public(string topic)
        {
            return new WebSocketTopic(topic, WebSocketTopicKind.Public);
        }

        public static WebSocketTopic Unspecified(string topic)
        {
            return new WebSocketTopic(topic, WebSocketTopicKind.Unspecified);
        }

        protected bool Equals(WebSocketTopic other)
        {
            return string.Equals(Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WebSocketTopic)obj);
        }

        public override int GetHashCode()
        {
            return (Key != null ? Key.GetHashCode() : 0);
        }
    }
}
