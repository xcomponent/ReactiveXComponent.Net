using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Parser
{
    public class XCApiConfigParser : IXCApiConfigParser
    {
        private XmlDocument _document;
        private XCApiDescription _xcApiDescription;
        
        private Dictionary<string, long> _componentCodeByComponent;
        private Dictionary<string, Dictionary<string, long>> _stateMachineCodeByStateMachineAndComponent;
        private Dictionary<string, int> _eventCodeByEvent;
        private Dictionary<TopicIdentifier, string> _publisherTopicByIdentifier;
        private Dictionary<TopicIdentifier, string> _consumerTopicByIdentifier;

        public XCApiConfigParser()
        {
        }

        public void Parse(Stream xcApiStream)
        {
            var reader = XmlReader.Create(xcApiStream);
            _document = new XmlDocument();
            _document.Load(reader);
            _xcApiDescription = new XCApiDescription(_document);

            _componentCodeByComponent = CreateComponentCodeByNameDico(_xcApiDescription.GetComponentsNode());
            _stateMachineCodeByStateMachineAndComponent = CreateStateMachineCodeByNameAndComponentDico(_xcApiDescription.GetComponentsNode());
            _eventCodeByEvent = CreateEventCodeByEventDico(_xcApiDescription.GetPublishersNode());
            _publisherTopicByIdentifier = CreatePublisherTopicByComponentStateMachineAndEvenCodeDico(_xcApiDescription.GetPublishersNode());
            _consumerTopicByIdentifier = CreateConsumerTopicByComponentStateMachineAndEvenCodeDico(_xcApiDescription.GetConsumersNode());
        }

        private Dictionary<string, long> CreateComponentCodeByNameDico(XmlNodeList components)
        {
            Dictionary<string, long> componentCodeByName = new Dictionary<string, long>();

            foreach (XmlElement component in components)
            {
                AddComponentToDictionary(componentCodeByName, component);
            }
            return componentCodeByName; 
        }

        private void AddComponentToDictionary(Dictionary<String, long> dictionnary, XmlElement component)
        {
            dictionnary.Add(component.Attributes[XCApiTags.Name].Value, Convert.ToInt64(component.Attributes[XCApiTags.Id].Value));
        } 

        private Dictionary<string, Dictionary<string, long>> CreateStateMachineCodeByNameAndComponentDico(XmlNodeList components)
        {
            Dictionary<string, Dictionary<string, long>> stateMachineCodeDico = new Dictionary<string, Dictionary<string, long>>();
            foreach (XmlElement component in components)
            {
                stateMachineCodeDico.Add(component.Attributes[XCApiTags.Name].Value, new Dictionary<string, long>());
                AddStateMachineToDictionary(stateMachineCodeDico[component.Attributes[XCApiTags.Name].Value], component);
            }
            return stateMachineCodeDico;
        }

        private void AddStateMachineToDictionary(Dictionary<string, long> dictionary, XmlElement component)
        {
            foreach (XmlElement stateMachine in component.LastChild)
            {
                dictionary.Add(stateMachine.Attributes[XCApiTags.Name].Value, Convert.ToInt64(stateMachine.Attributes[XCApiTags.Id].Value));
            }
        }

        private Dictionary<string, int> CreateEventCodeByEventDico(XmlNodeList publishNodes)
        {
            Dictionary<string, int> eventCodeByEvent = new Dictionary<string, int>();

            foreach (XmlNode node in publishNodes)
            {
                AddEventCodeToDictionary(eventCodeByEvent, node);
            }
            return eventCodeByEvent;
        }

        private void AddEventCodeToDictionary(Dictionary<string, int> dictionary, XmlNode node)
        {
            if (!dictionary.ContainsKey(node?.Attributes[XCApiTags.EventName].Value))
            {
                dictionary.Add(node.Attributes[XCApiTags.EventName].Value,
                    Convert.ToInt32(node.Attributes[XCApiTags.EventCode].Value));
            }
        }

        private Dictionary<TopicIdentifier, string> CreatePublisherTopicByComponentStateMachineAndEvenCodeDico(XmlNodeList publishNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifier = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in publishNodes)
            {
                AddTopicToDictionary(topicByIdentifier, node);
            }
            return topicByIdentifier;
        }

        private Dictionary<TopicIdentifier, string> CreateConsumerTopicByComponentStateMachineAndEvenCodeDico(XmlNodeList subscribeNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifier = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in subscribeNodes)
            {
                AddTopicToDictionary(topicByIdentifier, node);
            }
            return topicByIdentifier;
        }

        private void AddTopicToDictionary(Dictionary<TopicIdentifier, string> dictionary, XmlNode node)
        {
            var componentCode = Convert.ToInt64(node?.Attributes[XCApiTags.ComponentCode]?.Value);
            var stateMachineCode = Convert.ToInt64(node?.Attributes[XCApiTags.StateMachineCode]?.Value);
            var eventCode = node?.Attributes[XCApiTags.EventCode] == null
                ? 0 : Convert.ToInt32(node.Attributes[XCApiTags.EventCode].Value);
            var topicType = node?.Attributes[XCApiTags.TopicType]?.Value;

            var topicIdentifier = CreateTopicIdentifier(componentCode, stateMachineCode, eventCode, topicType);

            if (!dictionary.ContainsKey(topicIdentifier))
            {
                XmlNode topicNode = node?.FirstChild;
                dictionary.Add(topicIdentifier, topicNode?.InnerText);
            }
        }

        private TopicIdentifier CreateTopicIdentifier(long componentCode, long stateMachineCode, int eventCode, string topicType)
        {
            var topicIdentifier = new TopicIdentifier
            {
                Component = componentCode,
                StateMachine = stateMachineCode,
                EventCode = eventCode,
                TopicType = topicType
            };
            return topicIdentifier;
        }

        public string GetConnectionType()
        {
            XmlNode connection = _xcApiDescription.GetCommunicationNode()?.Item(0)?.FirstChild;
            return connection?.LocalName;
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
                TopicType = XCApiTags.Output
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
                TopicType = XCApiTags.Input
            };
            _consumerTopicByIdentifier.TryGetValue(topicId, out consumerTopic);
            return consumerTopic;
        }
    }
}
