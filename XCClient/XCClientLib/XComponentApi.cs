using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCClientLib.Common;

namespace XCClientLib
{
    using XCClientLib.Parser;
    using XCClientLib.RabbitMQ;

    using XComponent.Common.Helper;

    public class XComponentApi : IXComponentApi
    {
        private readonly IRabbitMqPublisherFactory publisherFactory;

        private readonly IRabbitMQConsumerFactory consumerFactory;

        private readonly IRabbitMqConnection connection;

        private readonly DeploymentParser parser;

        private Dictionary<Action<MessageEventArgs>, IConsumer> callbacks;
        private Dictionary<ConsumerKey,List<Action<MessageEventArgs>>> callbacksByConsumerKey;
        private List<Action<MessageEventArgs>> callbacksList; 
        private IConsumer _privateConsumer;
        private ConsumerKey consumerKey;

        public static string PrivateCommunicationIdentifier { get; set; }

        public XComponentApi(IRabbitMqPublisherFactory publisherFactory, IRabbitMQConsumerFactory consumerFactory, IRabbitMqConnection connection, DeploymentParser parser)
        {
            this.publisherFactory = publisherFactory;
            this.consumerFactory = consumerFactory;
            this.connection = connection;
            this.parser = parser;
            this.callbacks = new Dictionary<Action<MessageEventArgs>, IConsumer>();
            callbacksByConsumerKey = new Dictionary<ConsumerKey, List<Action<MessageEventArgs>>>();
        }

        public void SendEvent(string engine, string component, string stateMachine, int eventCode, string messageType, object message, Visibility visibility)
        {
            Header header = new Header()
                {
                    ComponentCode = HashcodeHelper.GetXcHashCode(component),
                    EngineCode = HashcodeHelper.GetXcHashCode(engine),
                    StateMachineCode = HashcodeHelper.GetXcHashCode(stateMachine),
                    MessageType = messageType,
                    EventCode = eventCode,
                    PublishTopic = string.Empty
              
                };

            if (visibility == Visibility.Private)
            {
                header.PublishTopic = PrivateCommunicationIdentifier;
            }
            var publisher = this.publisherFactory.Create(component);
            publisher.Send(header, message, this.parser.GetPublisherTopic(component, stateMachine, eventCode));
            publisher.Close();
        }

        public void AddCallback(string component, string stateMachine, Action<MessageEventArgs> callback)
        {
            var topic = parser.GetConsumerTopic(component, stateMachine);

            IConsumer consumer = this.consumerFactory.Create(component, topic);
            this.callbacks.Add(callback, consumer);
            consumer.MessageReceived += (sender, args) => callback(args);
            consumer.Start();

            callbacksList = new List<Action<MessageEventArgs>>();
            consumerKey = new ConsumerKey(HashcodeHelper.GetXcHashCode(component), HashcodeHelper.GetXcHashCode(stateMachine));
            callbacksByConsumerKey.Add(consumerKey, callbacksList);
            callbacksByConsumerKey[consumerKey].Add(callback);
            if (PrivateCommunicationIdentifier != null)
                InitPrivateConsumer(component, stateMachine);
        }

        public void InitPrivateConsumer(string component, string stateMachine)
        {
            _privateConsumer = this.consumerFactory.Create(component, PrivateCommunicationIdentifier);
            _privateConsumer.MessageReceived += (sender, args) =>
            {
                if (args.Header.StateMachineCode == HashcodeHelper.GetXcHashCode(stateMachine) 
                        && args.Header.ComponentCode == HashcodeHelper.GetXcHashCode(component))
                {
                    var localconsumerKey = new ConsumerKey(args.Header.ComponentCode, args.Header.StateMachineCode);
                    List<Action<MessageEventArgs>> localCallBacks;
                    if (callbacksByConsumerKey.TryGetValue(localconsumerKey, out localCallBacks))
                    {
                        foreach (var callBack in localCallBacks)
                        {
                            callBack(args);
                        }
                    }
                }
                
            };
            _privateConsumer.Start();

        }

        public void RemoveCallback(Action<MessageEventArgs> callback)
        {
            if (this.callbacks.ContainsKey(callback))
            {
                this.callbacks[callback].Stop();
                this.callbacks.Remove(callback);
            }
            var actions = callbacksByConsumerKey.Values.ToList();
            foreach (var action in actions)
            {
                if (action.Contains(callback))
                {
                    action.Remove(callback);
                }
            }
        }

        public void Close()
        {
            this.callbacks.Values.ToList().ForEach(e => e.Stop());
            this.callbacks.Clear();
            if (PrivateCommunicationIdentifier != null)
            { 
                _privateConsumer.Stop();
                this.callbacksByConsumerKey.Clear();
            }
            this.connection.Close();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
