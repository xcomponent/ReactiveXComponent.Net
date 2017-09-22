using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;

namespace ReactiveXComponent.Parser
{
    public class XCApiConfigParser : IXCApiConfigParser
    {
        private XCApiDescription _xcApiDescription;

        private Dictionary<string, int> _componentCodeByComponent;
        private Dictionary<string, int> _stateMachineCodeByStateMachine;
        private Dictionary<string, int> _eventCodeByEvent;
        private Dictionary<TopicIdentifier, string> _publisherTopicByIdentifier;
        private Dictionary<TopicIdentifier, string> _subscriberTopicByIdentifier;
        private Dictionary<long, string> _snapshotTopicByComponent;

        private XNamespace _xc;

        public void Parse(Stream xcApiStream)
        {
            var reader = XmlReader.Create(xcApiStream);
            _xcApiDescription = new XCApiDescription(reader);
            _xc = "http://xcomponent.com/DeploymentConfig.xsd";

            _componentCodeByComponent = CreateComponentCodeByNameRepository(_xcApiDescription.GetComponentsNode());
            _stateMachineCodeByStateMachine = CreateStateMachineCodeByNameRepository(_xcApiDescription.GetStateMachinesNode());
            _eventCodeByEvent = CreateEventCodeByEventRepository(_xcApiDescription.GetPublishersNode());
            _publisherTopicByIdentifier = CreatePublisherTopicByComponentAndStateMachineRepository(_xcApiDescription.GetPublishersNode());
            _subscriberTopicByIdentifier = CreateConsumerTopicByComponentAndStateMachineAndRepository(_xcApiDescription.GetConsumersNode());
            _snapshotTopicByComponent = CreateSnapshotTopicByComponentRepository(_xcApiDescription.GetSnapshotsNode()); ;
        }

        private Dictionary<string, int> CreateComponentCodeByNameRepository(IEnumerable<XElement> components)
        {
            Dictionary<string, int> componentCodeByNameRepo = new Dictionary<string, int>();

            foreach (XElement component in components)
            {
                AddComponentToRepository(componentCodeByNameRepo, component);
            }
            return componentCodeByNameRepo;
        }

        private void AddComponentToRepository(Dictionary<string, int> repository, XElement component)
        {
            repository.Add(component.Attribute(XCApiTags.Name).Value, Convert.ToInt32(component.Attribute(XCApiTags.Id).Value));
        }

        private Dictionary<string, int> CreateStateMachineCodeByNameRepository(IEnumerable<XElement> stateMachines)
        {
            Dictionary<string, int> stateMachineCodeRepo = new Dictionary<string, int>();
            foreach (XElement stateMachine in stateMachines)
            {
                stateMachineCodeRepo.Add(stateMachine.Attribute(XCApiTags.Name).Value, Convert.ToInt32(stateMachine.Attribute(XCApiTags.Id).Value));
            }
            return stateMachineCodeRepo;
        }

        private Dictionary<string, int> CreateEventCodeByEventRepository(IEnumerable<XElement> publishNodes)
        {
            Dictionary<string, int> eventCodeByEventRepo = new Dictionary<string, int>();

            foreach (XElement node in publishNodes)
            {
                AddEventCodeToRepository(eventCodeByEventRepo, node);
            }
            return eventCodeByEventRepo;
        }

        private void AddEventCodeToRepository(Dictionary<string, int> repository, XElement node)
        {
            if (node?.Attribute(XCApiTags.EventName)?.Value != null && !repository.ContainsKey(node.Attribute(XCApiTags.EventName).Value))
            {
                repository.Add(node.Attribute(XCApiTags.EventName).Value,
                    Convert.ToInt32(node.Attribute(XCApiTags.EventCode).Value));
            }
        }

        private Dictionary<TopicIdentifier, string> CreatePublisherTopicByComponentAndStateMachineRepository(IEnumerable<XElement> publishNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifierRepo = new Dictionary<TopicIdentifier, string>();

            foreach (XElement node in publishNodes)
            {
                AddTopicToRepository(topicByIdentifierRepo, node);
            }
            return topicByIdentifierRepo;
        }

        private Dictionary<TopicIdentifier, string> CreateConsumerTopicByComponentAndStateMachineAndRepository(IEnumerable<XElement> subscribeNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifierRepo = new Dictionary<TopicIdentifier, string>();

            foreach (XElement node in subscribeNodes)
            {
                AddTopicToRepository(topicByIdentifierRepo, node);
            }
            return topicByIdentifierRepo;
        }

        private void AddTopicToRepository(Dictionary<TopicIdentifier, string> repository, XElement node)
        {
            var componentCode = Convert.ToInt64(node?.Attribute(XCApiTags.ComponentCode)?.Value);
            var stateMachineCode = Convert.ToInt64(node?.Attribute(XCApiTags.StateMachineCode)?.Value);
            var topicType = node?.Attribute(XCApiTags.TopicType)?.Value;

            var topicIdentifier = new TopicIdentifier(componentCode, stateMachineCode, topicType);

            if (!repository.ContainsKey(topicIdentifier))
            {
                repository.Add(topicIdentifier, node?.Descendants(_xc + XCApiTags.Topic).FirstOrDefault().Value);
            }
        }

        private Dictionary<long, string> CreateSnapshotTopicByComponentRepository(IEnumerable<XElement> snapshotNodes)
        {
            Dictionary<long, string> SnapshotTopicByComponentRepo = new Dictionary<long, string>();
            foreach (XElement node in snapshotNodes)
            {
                AddSnapshotTopicToRepository(SnapshotTopicByComponentRepo, node);
            }
            return SnapshotTopicByComponentRepo;
        }

        private void AddSnapshotTopicToRepository(Dictionary<long, string> repository, XElement node)
        {
            var componentCode = Convert.ToInt64(node?.Attribute(XCApiTags.ComponentCode)?.Value);

            if (!repository.ContainsKey(componentCode))
            {
                repository.Add(componentCode, node?.Descendants(_xc + XCApiTags.Topic).FirstOrDefault().Value);
            }
        }

        public string GetConnectionType()
        {
            var commmunicationNode = _xcApiDescription.GetCommunicationNode()?.FirstOrDefault();

            if (commmunicationNode != null)
            {
                var communicationChildElements = commmunicationNode.Elements().ToList();

                if (communicationChildElements != null && communicationChildElements.Count == 1)
                {
                    var connection = communicationChildElements.FirstOrDefault();
                    return connection.Name.LocalName;
                }
            }

            return string.Empty;
        }

        public string GetSerializationType()
        {
            var serialisation = _xcApiDescription.GetSerializationNode()?.FirstOrDefault()?.Value;
            return serialisation;
        }

        public BusDetails GetBusDetails()
        {
            XElement busInfos = _xcApiDescription.GetBusNode()?.FirstOrDefault();
            var busDetails = new BusDetails(
                    busInfos?.Attribute("user")?.Value,
                    busInfos?.Attribute("password")?.Value,
                    busInfos?.Attribute("host")?.Value,
                    Convert.ToInt32(busInfos?.Attribute("port")?.Value));

            return busDetails;
        }

        public WebSocketEndpoint GetWebSocketEndpoint()
        {
            XElement websocketInfos = _xcApiDescription.GetWebSocketNode()?.FirstOrDefault();

            WebSocketType webSocketType;
            var webSocketTypeString = websocketInfos?.Attribute("type")?.Value;
            if (!Enum.TryParse(webSocketTypeString, out webSocketType))
            {
                throw new ReactiveXComponentException($"Could not parse communication type: {webSocketTypeString}");
            }

            var webSocketEndpoint = new WebSocketEndpoint(
                 websocketInfos?.Attribute("name")?.Value,
                 websocketInfos?.Attribute("host")?.Value,
                 websocketInfos?.Attribute("port")?.Value,
                 webSocketType);

            return webSocketEndpoint;
        }

        public int GetComponentCode(string component)
        {
            int componentCode;
            _componentCodeByComponent.TryGetValue(component, out componentCode);
            return componentCode;
        }

        public int GetStateMachineCode(string component, string stateMachine)
        {
            int stateMachineCode = 0;
            _stateMachineCodeByStateMachine?.TryGetValue(stateMachine, out stateMachineCode);
            return stateMachineCode;
        }

        public int GetPublisherEventCode(string eventName)
        {
            int eventCode;
            _eventCodeByEvent.TryGetValue(eventName, out eventCode);
            return eventCode;
        }

        public string GetPublisherTopic(string component, string stateMachine)
        {
            string publisherTopic;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = GetStateMachineCode(component, stateMachine);
            var topicId = new TopicIdentifier(componentCode, stateMachineCode, XCApiTags.Output);

            _publisherTopicByIdentifier.TryGetValue(topicId, out publisherTopic);
            return publisherTopic;
        }

        public string GetPublisherTopic(long componentCode, long stateMachineCode)
        {
            string publisherTopic;
            var topicId = new TopicIdentifier(componentCode, stateMachineCode, XCApiTags.Output);

            _publisherTopicByIdentifier.TryGetValue(topicId, out publisherTopic);

            return publisherTopic;
        }

        public string GetSubscriberTopic(string component, string stateMachine)
        {
            string subscriberTopic;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = GetStateMachineCode(component, stateMachine);
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
