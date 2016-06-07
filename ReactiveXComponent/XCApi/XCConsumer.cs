using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public class XCConsumer : IXCConsumer
    {
        private bool _disposed = false;

        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly DeploymentParser _parser;
        private readonly Dictionary<Action<MessageEventArgs>, IConsumer> _callbacks;
        private readonly Dictionary<ConsumerKey, List<Action<MessageEventArgs>>> _callbacksByConsumerKey;
        private List<Action<MessageEventArgs>> _callbacksList;
        private IConsumer _privateConsumer;
        private IConsumer _publicConsumer;
        private ConsumerKey _consumerKey;
        private string _privateCommunicationIdentifier;

        public XCConsumer(IRabbitMqConsumerFactory consumerFactory, DeploymentParser parser)
        {
            _consumerFactory = consumerFactory;
            _parser = parser;
            _callbacks = new Dictionary<Action<MessageEventArgs>, IConsumer>();
            _callbacksByConsumerKey = new Dictionary<ConsumerKey, List<Action<MessageEventArgs>>>(); 
        }

        public void CreateConsummer(string component, string stateMachine )
        {
            var topic = _parser.GetConsumerTopic(component, stateMachine);
            _publicConsumer = _consumerFactory.Create(component, topic);

            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                _privateConsumer = _consumerFactory.Create(component, _privateCommunicationIdentifier);
            }
        }

        public void InitPrivateCommunication(string privateCommunicationIdentifier)
        {
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
        }

        public void AddCallback(string component, string stateMachine, Action<MessageEventArgs> callback)
        {
            _callbacks.Add(callback, _publicConsumer);
            _publicConsumer.MessageReceived += (sender, args) => callback(args);
            _publicConsumer.Start();

            _callbacksList = new List<Action<MessageEventArgs>>();
            _consumerKey = new ConsumerKey(HashcodeHelper.GetXcHashCode(component), HashcodeHelper.GetXcHashCode(stateMachine));
            if (_callbacksByConsumerKey.ContainsKey(_consumerKey))
            {
                _callbacksByConsumerKey[_consumerKey].Add(callback);
            }
            else
            {
                _callbacksByConsumerKey.Add(_consumerKey, _callbacksList);
            } 
            if (!string.IsNullOrEmpty(_privateCommunicationIdentifier))
            {
                AddPrivateCallback(component, stateMachine, callback);
            }
        }

        private void AddPrivateCallback(string component, string stateMachine, Action<MessageEventArgs> callback)
        {
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
            if (_callbacks.ContainsKey(callback))
            {
                _callbacks[callback].Stop();
                _callbacks.Remove(callback);
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
            _callbacks.Values.ToList().ForEach(e => e.Stop());
            _callbacks.Clear();
            if (_privateCommunicationIdentifier != null)
            {
                _privateConsumer.Stop();
                _callbacksByConsumerKey.Clear();
            }
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
                return;
            else
            {
                Close();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(_disposed);
            GC.SuppressFinalize(this);
        }
    }
}
