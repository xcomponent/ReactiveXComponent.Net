using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketSubscription
    {
        public WebSocketTopic Topic { get; set; }

        public WebSocketSubscription()
        {
        }

        public WebSocketSubscription(WebSocketTopic topic)
        {
            Topic = topic;
        }

        protected bool Equals(WebSocketSubscription other)
        {
            return Equals(Topic, other.Topic);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WebSocketSubscription)obj);
        }

        public override int GetHashCode()
        {
            return (Topic != null ? Topic.GetHashCode() : 0);
        }
    }
}
