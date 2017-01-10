using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqSubscriberInfos
    {
        public RabbitMqSubscriberInfos()
        {
            Handlers = new List<EventHandler<BasicDeliverEventArgs>>();
        }

        public RabbitMqSubscriberInfos(IModel channel, EventingBasicConsumer subscriber)
        {
            Channel = channel;
            Subscriber = subscriber;
            Handlers = new List<EventHandler<BasicDeliverEventArgs>>();
        }

        public RabbitMqSubscriberInfos(IModel channel, EventingBasicConsumer subscriber, EventHandler<BasicDeliverEventArgs> handler)
        {
            Channel = channel;
            Subscriber = subscriber;
            Subscriber.Received += handler;
            Handlers = new List<EventHandler<BasicDeliverEventArgs>>() { handler };
        }

        public void AddHandler(EventHandler<BasicDeliverEventArgs> handler)
        {
            Subscriber.Received += handler;
            Handlers.Add(handler);
        }

        public IModel Channel { get; set; }
        public EventingBasicConsumer Subscriber { get; set; }
        public List<EventHandler<BasicDeliverEventArgs>> Handlers { get; private set; } 
    }
}
