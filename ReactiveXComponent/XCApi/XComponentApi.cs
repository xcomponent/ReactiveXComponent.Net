using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public class XComponentApi : IXComponentApi
    {
        private readonly IRabbitMqPublisherFactory _publisherFactory;

        private readonly IRabbitMqConsumerFactory _consumerFactory;

        private readonly IRabbitMqConnection _connection;

        private readonly DeploymentParser _parser;

        private Dictionary<Action<MessageEventArgs>, IConsumer> _callbacks;
        private Dictionary<ConsumerKey,List<Action<MessageEventArgs>>> _callbacksByConsumerKey;
        private List<Action<MessageEventArgs>> _callbacksList; 
        private IConsumer _privateConsumer;
        private ConsumerKey _consumerKey;

        public static string PrivateCommunicationIdentifier { get; set; }

        public XComponentApi(IRabbitMqPublisherFactory publisherFactory, IRabbitMqConsumerFactory consumerFactory, IRabbitMqConnection connection, DeploymentParser parser)
        {
            _publisherFactory = publisherFactory;
            _consumerFactory = consumerFactory;
            _connection = connection;
            _parser = parser;
            _callbacks = new Dictionary<Action<MessageEventArgs>, IConsumer>();
            _callbacksByConsumerKey = new Dictionary<ConsumerKey, List<Action<MessageEventArgs>>>();
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
 
            var publisher = this._publisherFactory.Create(component);
            publisher.Send(header, message, this._parser.GetPublisherTopic(component, stateMachine, eventCode));
            publisher.Close();
        }

        public void AddCallback(string component, string stateMachine, Action<MessageEventArgs> callback)
        {
            var topic = _parser.GetConsumerTopic(component, stateMachine);

            IConsumer consumer = this._consumerFactory.Create(component, topic);
            this._callbacks.Add(callback, consumer);
            consumer.MessageReceived += (sender, args) => callback(args);
            consumer.Start();

            _callbacksList = new List<Action<MessageEventArgs>>();
            _consumerKey = new ConsumerKey(HashcodeHelper.GetXcHashCode(component), HashcodeHelper.GetXcHashCode(stateMachine));
            _callbacksByConsumerKey.Add(_consumerKey, _callbacksList);
            _callbacksByConsumerKey[_consumerKey].Add(callback);
            if (PrivateCommunicationIdentifier != null)
                InitPrivateConsumer(component, stateMachine);
        }

        public void InitPrivateConsumer(string component, string stateMachine)
        {
            _privateConsumer = this._consumerFactory.Create(component, PrivateCommunicationIdentifier);
            _privateConsumer.MessageReceived += (sender, args) =>
            {
                if (args.Header.StateMachineCode == HashcodeHelper.GetXcHashCode(stateMachine) 
                        && args.Header.ComponentCode == HashcodeHelper.GetXcHashCode(component))
                {
                    var localconsumerKey = new ConsumerKey(args.Header.ComponentCode, args.Header.StateMachineCode);
                    List<Action<MessageEventArgs>> localCallBacks;
                    if (_callbacksByConsumerKey.TryGetValue(localconsumerKey, out localCallBacks))
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
            if (this._callbacks.ContainsKey(callback))
            {
                this._callbacks[callback].Stop();
                this._callbacks.Remove(callback);
            }
            var actions = _callbacksByConsumerKey.Values.ToList();
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
            this._callbacks.Values.ToList().ForEach(e => e.Stop());
            this._callbacks.Clear();
            if (PrivateCommunicationIdentifier != null)
            { 
                _privateConsumer.Stop();
                this._callbacksByConsumerKey.Clear();
            }
            this._connection.Close();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
