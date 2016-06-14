using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Parser
{
    public class Parser
    {
        private readonly XDocument _document;
        private Tags _tags;
        
        private Dictionary<string, long> _componentCodeByComponent;
        private Dictionary<string, Dictionary<string, long>> _stateMachineCodeByStateMachineAndComponent;
        private Dictionary<string, int> _eventCodeByEvent;
        private Dictionary<TopicIdentifier, string> _publisherTopicByIdentifier;
        private Dictionary<TopicIdentifier, string> _consumerTopicByIdentifier;

        public Parser(Stream file)
        {
             var reader = XmlReader.Create(file);
            _document = XDocument.Load(reader);
        }

        public void Parse(Tags tags)
        {
            _tags = tags;
            var xmlFromXDoc = new XDocumentConverter(_document.ToString());

            _componentCodeByComponent = CreateComponentCodeByNameDico(xmlFromXDoc.GetComponentsNode());
            _stateMachineCodeByStateMachineAndComponent = CreateStateMachineCodeByNameAndComponentDico(xmlFromXDoc.GetComponentsNode(),
                xmlFromXDoc.GetStateMachinesNode());
            _eventCodeByEvent = CreateEventCodeByEventDico(xmlFromXDoc.GetPublishersNode());
            _publisherTopicByIdentifier = CreatePublisherTopicByComponentStateMachineAndEvenCodeDico(xmlFromXDoc.GetPublishersNode());
            _consumerTopicByIdentifier = CreateConsumerTopicByComponentStateMachineAndEvenCodeDico(xmlFromXDoc.GetConsumersNode());

        }

        private Dictionary<string, long> CreateComponentCodeByNameDico(XmlNodeList components)
        {
            Dictionary<string, long> componentCodeByName = new Dictionary<string, long>();

            foreach (XmlElement component in components)
            {
                componentCodeByName.Add(component.Attributes[_tags.Name].Value, Convert.ToInt64(component.Attributes[_tags.Id].Value));
            }
            return componentCodeByName; 
        }

        private Dictionary<string, Dictionary<string, long>> CreateStateMachineCodeByNameAndComponentDico(XmlNodeList components, XmlNodeList stateMachines)
        {
            Dictionary<string, Dictionary<string, long>> stateMachineCodeDico = new Dictionary<string, Dictionary<string, long>>();
            int i = 0;
            foreach (XmlElement component in components)
            {
                stateMachineCodeDico.Add(component.Attributes[_tags.Name].Value, new Dictionary<string, long>());
                foreach (XmlElement stateMachine in stateMachines[i])
                {
                    stateMachineCodeDico[component.Attributes[_tags.Name].Value].Add(stateMachine.Attributes[_tags.Name].Value, Convert.ToInt64(stateMachine.Attributes[_tags.Id].Value));
                }
                i++;
            }
            return stateMachineCodeDico;
        }

        private Dictionary<string, int> CreateEventCodeByEventDico(XmlNodeList publishNodes)
        {
            Dictionary<string, int> eventCodeByEvent = new Dictionary<string, int>();

            foreach (XmlNode node in publishNodes)
            {
                if (!eventCodeByEvent.ContainsKey(node?.Attributes[_tags.EventName].Value))
                {
                    eventCodeByEvent.Add(node.Attributes[_tags.EventName].Value,
                        Convert.ToInt32(node.Attributes[_tags.EventCode].Value));
                }
            }
            return eventCodeByEvent;
        }

        private Dictionary<TopicIdentifier, string> CreatePublisherTopicByComponentStateMachineAndEvenCodeDico(XmlNodeList publishNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifier = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in publishNodes)
            {
                var topicIdentifier = new TopicIdentifier
                {
                    Component = Convert.ToInt64(node?.Attributes[_tags.ComponentCode]?.Value),
                    StateMachine = Convert.ToInt64(node?.Attributes[_tags.StateMachineCode]?.Value),
                    EventCode = Convert.ToInt64(node?.Attributes[_tags.EventCode]?.Value),
                    TopicType = node?.Attributes[_tags.TopicType]?.Value
                };
                if (!topicByIdentifier.ContainsKey(topicIdentifier))
                {
                    foreach (XmlNode topicNode in node?.ChildNodes)
                    {
                        topicByIdentifier.Add(topicIdentifier, topicNode.InnerText);
                    }
                }
            }
            return topicByIdentifier;
        }

        private Dictionary<TopicIdentifier, string> CreateConsumerTopicByComponentStateMachineAndEvenCodeDico(XmlNodeList subscribeNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifier = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in subscribeNodes)
            {
                var topicIdentifier = new TopicIdentifier
                {
                    Component = Convert.ToInt64(node?.Attributes[_tags.ComponentCode]?.Value),
                    StateMachine = Convert.ToInt64(node?.Attributes[_tags.StateMachineCode]?.Value),
                    EventCode = 0,
                    TopicType = node?.Attributes[_tags.TopicType]?.Value
                };

                                                         
                if (!topicByIdentifier.ContainsKey(topicIdentifier))
                {
                    foreach (XmlNode topicNode in node?.ChildNodes)
                    {
                        topicByIdentifier.Add(topicIdentifier, topicNode.InnerText);
                    }
                }
            }
            return topicByIdentifier;
        }


        public string GetConnectionType()
        {
            return (from communication in _document.Descendants()
                    where communication?.Name.LocalName == _tags.Bus || communication?.Name.LocalName == _tags.Websocket
                    select communication.Name.LocalName).FirstOrDefault();
        }

        public long GetComponentCode(string component)
        {
            long componentCode = 0;
            _componentCodeByComponent.TryGetValue(component, out componentCode);
            return componentCode;
        }

        public long GetStateMachineCode(string component, string stateMachine)
        {
            Dictionary<string, long> stateMachinesByComponentDico = null;
            long stateMachineCode = 0;
            _stateMachineCodeByStateMachineAndComponent?.TryGetValue(component, out stateMachinesByComponentDico);
            stateMachinesByComponentDico?.TryGetValue(stateMachine, out stateMachineCode);
            return stateMachineCode;
        }

        public int GetPublisherEventCode(string eventName)
        {
            int eventCode = 0;
            _eventCodeByEvent.TryGetValue(eventName, out eventCode);
            return eventCode ;
        }

        public string GetPublisherTopic(string component, string stateMachine, int eventCode)
        {
            string publisherTopic = String.Empty;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = GetStateMachineCode(component, stateMachine);
            var topicId = new TopicIdentifier
            {
                Component = componentCode,
                StateMachine = stateMachineCode,
                EventCode = eventCode,
                TopicType = _tags.Output
            };
            _publisherTopicByIdentifier.TryGetValue(topicId, out publisherTopic);
            return publisherTopic;
        }

        public string GetConsumerTopic(string component, string stateMachine)
        {
            string consumerTopic = String.Empty;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = GetStateMachineCode(component, stateMachine);
            var topicId = new TopicIdentifier
            {
                Component = componentCode,
                StateMachine = stateMachineCode,
                EventCode = 0,
                TopicType = _tags.Input
            };
            _consumerTopicByIdentifier.TryGetValue(topicId, out consumerTopic);
            return consumerTopic;
        }
    }
}
