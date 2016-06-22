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

        public void Parse(Stream xcApiStream)
        {
            var reader = XmlReader.Create(xcApiStream);
            _document = new XmlDocument();
            _document.Load(reader);
            _xcApiDescription = new XCApiDescription(_document);

            _componentCodeByComponent = CreateComponentCodeByNameRepository(_xcApiDescription.GetComponentsNode());
            _stateMachineCodeByStateMachineAndComponent = CreateStateMachineCodeByNameAndComponentRepository(_xcApiDescription.GetComponentsNode());
            _eventCodeByEvent = CreateEventCodeByEventRepository(_xcApiDescription.GetPublishersNode());
            _publisherTopicByIdentifier = CreatePublisherTopicByComponentStateMachineAndEvenCodeRepository(_xcApiDescription.GetPublishersNode());
            _consumerTopicByIdentifier = CreateConsumerTopicByComponentStateMachineAndEvenCodeRepository(_xcApiDescription.GetConsumersNode());
        }

        private Dictionary<string, long> CreateComponentCodeByNameRepository(XmlNodeList components)
        {
            Dictionary<string, long> componentCodeByNameRepo = new Dictionary<string, long>();

            foreach (XmlElement component in components)
            {
                AddComponentToRepository(componentCodeByNameRepo, component);
            }
            return componentCodeByNameRepo; 
        }

        private void AddComponentToRepository(Dictionary<String, long> repository, XmlElement component)
        {
            repository.Add(component.Attributes[XCApiTags.Name].Value, Convert.ToInt64(component.Attributes[XCApiTags.Id].Value));
        } 

        private Dictionary<string, Dictionary<string, long>> CreateStateMachineCodeByNameAndComponentRepository(XmlNodeList components)
        {
            Dictionary<string, Dictionary<string, long>> stateMachineCodeRepo = new Dictionary<string, Dictionary<string, long>>();
            foreach (XmlElement component in components)
            {
                stateMachineCodeRepo.Add(component.Attributes[XCApiTags.Name].Value, new Dictionary<string, long>());
                AddStateMachineToRepository(stateMachineCodeRepo[component.Attributes[XCApiTags.Name].Value], component);
            }
            return stateMachineCodeRepo;
        }

        private void AddStateMachineToRepository(Dictionary<string, long> repository, XmlElement component)
        {
            foreach (XmlElement stateMachine in component.LastChild)
            {
                repository.Add(stateMachine.Attributes[XCApiTags.Name].Value, Convert.ToInt64(stateMachine.Attributes[XCApiTags.Id].Value));
            }
        }

        private Dictionary<string, int> CreateEventCodeByEventRepository(XmlNodeList publishNodes)
        {
            Dictionary<string, int> eventCodeByEventRepo = new Dictionary<string, int>();

            foreach (XmlNode node in publishNodes)
            {
                AddEventCodeToRepository(eventCodeByEventRepo, node);
            }
            return eventCodeByEventRepo;
        }

        private void AddEventCodeToRepository(Dictionary<string, int> repository, XmlNode node)
        {
            if (node?.Attributes?[XCApiTags.EventName]?.Value != null && !repository.ContainsKey(node.Attributes[XCApiTags.EventName]?.Value))
            {
                repository.Add(node.Attributes[XCApiTags.EventName].Value,
                    Convert.ToInt32(node.Attributes[XCApiTags.EventCode].Value));
            }
        }

        private Dictionary<TopicIdentifier, string> CreatePublisherTopicByComponentStateMachineAndEvenCodeRepository(XmlNodeList publishNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifierRepo = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in publishNodes)
            {
                AddTopicToRepository(topicByIdentifierRepo, node);
            }
            return topicByIdentifierRepo;
        }

        private Dictionary<TopicIdentifier, string> CreateConsumerTopicByComponentStateMachineAndEvenCodeRepository(XmlNodeList subscribeNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifierRepo = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in subscribeNodes)
            {
                AddTopicToRepository(topicByIdentifierRepo, node);
            }
            return topicByIdentifierRepo;
        }

        private void AddTopicToRepository(Dictionary<TopicIdentifier, string> repository, XmlNode node)
        {
            var componentCode = Convert.ToInt64(node?.Attributes[XCApiTags.ComponentCode]?.Value);
            var stateMachineCode = Convert.ToInt64(node?.Attributes[XCApiTags.StateMachineCode]?.Value);
            var eventCode = node?.Attributes[XCApiTags.EventCode] == null
                ? 0 : Convert.ToInt32(node.Attributes[XCApiTags.EventCode].Value);
            var topicType = node?.Attributes[XCApiTags.TopicType]?.Value;

            var topicIdentifier = CreateTopicIdentifier(componentCode, stateMachineCode, eventCode, topicType);

            if (!repository.ContainsKey(topicIdentifier))
            {
                XmlNode topicNode = node?.FirstChild;
                repository.Add(topicIdentifier, topicNode?.InnerText);
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

        public BusDetails GetBusDetails()
        {
            BusDetails busDetails = null;
            XmlNode connection = _xcApiDescription.GetCommunicationNode().Item(0)?.FirstChild;
            if (connection?.Attributes != null)
            {
                busDetails = new BusDetails
                {
                    Host = connection.Attributes["host"]?.Value,
                    Username = connection.Attributes["user"]?.Value,
                    Password = connection.Attributes["password"]?.Value,
                    Port = Convert.ToInt32(connection.Attributes["port"]?.Value)
                };
            }
            return busDetails;
        }

        public long GetComponentCode(string component)
        {
            long componentCode;
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
            int eventCode;
            _eventCodeByEvent.TryGetValue(eventName, out eventCode);
            return eventCode ;
        }

        public string GetPublisherTopic(string component, string stateMachine, int eventCode)
        {
            string publisherTopic;
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
            string consumerTopic;
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
