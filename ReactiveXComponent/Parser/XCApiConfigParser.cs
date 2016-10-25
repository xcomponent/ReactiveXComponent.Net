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
        private HashSet<string> _eventNames;
        private Dictionary<TopicIdentifier, string> _publisherTopicByIdentifier;
        private Dictionary<TopicIdentifier, string> _subscriberTopicByIdentifier;
        private Dictionary<long, string> _snapshotTopicByComponent;

        public void Parse(Stream xcApiStream)
        {
            var reader = XmlReader.Create(xcApiStream);
            _document = new XmlDocument();
            _document.Load(reader);
            _xcApiDescription = new XCApiDescription(_document);

            _componentCodeByComponent = CreateComponentCodeByNameRepository(_xcApiDescription.GetComponentsNode());
            _stateMachineCodeByStateMachineAndComponent = CreateStateMachineCodeByNameAndComponentRepository(_xcApiDescription.GetComponentsNode());
            _eventCodeByEvent = CreateEventCodeByEventRepository(_xcApiDescription.GetPublishersNode());
            _snapshotTopicByComponent = CreateSnapshotTopicByComponentRepository(_xcApiDescription.GetSnapshotsNode()); ;
            _eventNames = CreateEventNamesRepository(_xcApiDescription.GetPublishersNode());
            _publisherTopicByIdentifier = CreatePublisherTopicByComponentAndStateMachineRepository(_xcApiDescription.GetPublishersNode());
            _subscriberTopicByIdentifier = CreateConsumerTopicByComponentAndStateMachineAndRepository(_xcApiDescription.GetConsumersNode());
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

        private HashSet<string> CreateEventNamesRepository(XmlNodeList publishNodes)
        {
            HashSet<string> eventNamesRepo = new HashSet<string>();

            foreach (XmlNode node in publishNodes)
            {
                AddEventNameToRepository(eventNamesRepo, node);
            }
            return eventNamesRepo;
        }

        private void AddEventCodeToRepository(Dictionary<string, int> repository, XmlNode node)
        {
            if (node?.Attributes?[XCApiTags.EventName]?.Value != null && !repository.ContainsKey(node.Attributes[XCApiTags.EventName]?.Value))
            {
                repository.Add(node.Attributes[XCApiTags.EventName].Value,
                    Convert.ToInt32(node.Attributes[XCApiTags.EventCode].Value));
            }
        }

        private void AddEventNameToRepository(HashSet<string> repository, XmlNode node)
        {
            if (node?.Attributes?[XCApiTags.EventName]?.Value != null && !repository.Contains(node.Attributes[XCApiTags.EventName]?.Value))
            {
                repository.Add(node.Attributes[XCApiTags.EventName].Value);
            }
        }

        private Dictionary<TopicIdentifier, string> CreatePublisherTopicByComponentAndStateMachineRepository(XmlNodeList publishNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifierRepo = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in publishNodes)
            {
                AddTopicToRepository(topicByIdentifierRepo, node);
            }
            return topicByIdentifierRepo;
        }

        private Dictionary<TopicIdentifier, string> CreateConsumerTopicByComponentAndStateMachineAndRepository(XmlNodeList subscribeNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifierRepo = new Dictionary<TopicIdentifier, string>();

            foreach (XmlNode node in subscribeNodes)
            {
                AddTopicToRepository(topicByIdentifierRepo, node);
            }
            return topicByIdentifierRepo;
        }

        private Dictionary<long, string> CreateSnapshotTopicByComponentRepository(XmlNodeList snapshotNodes)
        {
            Dictionary<long, string> SnapshotTopicByComponentRepo = new Dictionary<long, string>();
            foreach (XmlNode node in snapshotNodes)
            {
                AddSnapshotTopicToRepository(SnapshotTopicByComponentRepo, node);
            }
            return SnapshotTopicByComponentRepo;
        }

        private void AddTopicToRepository(Dictionary<TopicIdentifier, string> repository, XmlNode node)
        {
            var componentCode = Convert.ToInt64(node?.Attributes[XCApiTags.ComponentCode]?.Value);
            var stateMachineCode = Convert.ToInt64(node?.Attributes[XCApiTags.StateMachineCode]?.Value);
            var eventName = node?.Attributes[XCApiTags.EventName].Value;
            var topicType = node?.Attributes[XCApiTags.TopicType]?.Value;

            var topicIdentifier = CreateTopicIdentifier(componentCode, stateMachineCode, topicType);

            if (!repository.ContainsKey(topicIdentifier))
            {
                XmlNode topicNode = node?.FirstChild;
                repository.Add(topicIdentifier, topicNode?.InnerText);
            }
        }

        private void AddSnapshotTopicToRepository(Dictionary<long, string> repository, XmlNode node)
        {
            var componentCode = Convert.ToInt64(node?.Attributes[XCApiTags.ComponentCode]?.Value);

            if (!repository.ContainsKey(componentCode))
            {
                XmlNode topicNode = node?.FirstChild;
                repository.Add(componentCode, topicNode?.InnerText);
            }
        }
    
        private TopicIdentifier CreateTopicIdentifier(long componentCode, long stateMachineCode, string topicType)
        {
            var topicIdentifier = new TopicIdentifier(componentCode, stateMachineCode, topicType);
            return topicIdentifier;
        }

        public string GetConnectionType()
        {
            XmlNode connection = _xcApiDescription.GetCommunicationNode()?.Item(0)?.FirstChild;
            return connection?.LocalName;
        }

        public string GetSerializationType()
        {
            XmlNode serialisation = _xcApiDescription.GetSerializationNode()?.Item(0)?.FirstChild;
            return serialisation?.InnerText;
        }

        public BusDetails GetBusDetails()
        {
            BusDetails busDetails = null;
            XmlNode connection = _xcApiDescription.GetCommunicationNode().Item(0)?.FirstChild;
            if (connection?.Attributes != null)
            {
                busDetails = new BusDetails(
                    connection.Attributes["user"]?.Value,
                    connection.Attributes["password"]?.Value,
                    connection.Attributes["host"]?.Value,
                    Convert.ToInt32(connection.Attributes["port"]?.Value));
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

        public string GetPublisherTopic(string component, string stateMachine)
        {
            string publisherTopic;
            long code;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = Int64.TryParse(stateMachine, out code) ? code : GetStateMachineCode(component, stateMachine);
            var topicId = new TopicIdentifier(componentCode, stateMachineCode, XCApiTags.Output);

            _publisherTopicByIdentifier.TryGetValue(topicId, out publisherTopic);
            return publisherTopic;
        }

        public string GetSubscriberTopic(string component, string stateMachine)
        {
            string subscriberTopic;
            long code;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = Int64.TryParse(stateMachine, out code)? code : GetStateMachineCode(component, stateMachine);
            var topicId = new TopicIdentifier(componentCode, stateMachineCode, XCApiTags.Input);

            _subscriberTopicByIdentifier.TryGetValue(topicId, out subscriberTopic);
            return subscriberTopic;
        }

        public string GetSnapshotTopic(string component)
        {
            string snapshotTopic;
            var componentCode = GetComponentCode(component);
            _snapshotTopicByComponent.TryGetValue(componentCode, out snapshotTopic);
            return snapshotTopic;
        }
    }
}
