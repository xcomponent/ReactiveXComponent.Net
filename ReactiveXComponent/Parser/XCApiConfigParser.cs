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
        private XNamespace _xc;
        private Dictionary<string, int> _eventCodeByEvent;
        private Dictionary<TopicIdentifier, string> _publisherTopicByIdentifier;
        private Dictionary<TopicIdentifier, string> _subscriberTopicByIdentifier;
        private Dictionary<long, string> _snapshotTopicByComponent;
        private Dictionary<string, List<StateMachineInfo>> _stateMachineInfoByComponentRepository;
        private Dictionary<string, int> _componentCodeByComponentNameRepo;


        public void Parse(Stream xcApiStream)
        {
            var reader = XmlReader.Create(xcApiStream);
            _xcApiDescription = new XCApiDescription(reader);
            _xc = "http://xcomponent.com/DeploymentConfig.xsd";
            ParseComponentNodes(_xcApiDescription.GetComponentsNode(), out _componentCodeByComponentNameRepo, out _stateMachineInfoByComponentRepository);
            _eventCodeByEvent = CreateEventCodeByEventRepository(_xcApiDescription.GetPublishersNode());
            _publisherTopicByIdentifier = CreatePublisherTopicByComponentAndStateMachineRepository(_xcApiDescription.GetPublishersNode());
            _subscriberTopicByIdentifier = CreateConsumerTopicByComponentAndStateMachineAndRepository(_xcApiDescription.GetConsumersNode());
            _snapshotTopicByComponent = CreateSnapshotTopicByComponentRepository(_xcApiDescription.GetSnapshotsNode());
        }

        private void ParseComponentNodes(IEnumerable<XElement> components, 
            out Dictionary<string, int> componentCodeByComponentNameRepo, 
            out Dictionary<string, List<StateMachineInfo>> stateMachineInfoByComponentRepository)
        {
            componentCodeByComponentNameRepo = new Dictionary<string, int>();
            stateMachineInfoByComponentRepository = new Dictionary<string, List<StateMachineInfo>>();

            foreach (XElement component in components)
            {
                var componentName = component.Attribute(XCApiTags.Name)?.Value;
                if (string.IsNullOrEmpty(componentName))
                    continue;

                componentCodeByComponentNameRepo.Add(componentName, Convert.ToInt32(component.Attribute(XCApiTags.Id)?.Value));
                
                if (!_stateMachineInfoByComponentRepository.ContainsKey(componentName))
                {
                    var stateMachines = component.Descendants(_xc + "stateMachine");
                    var statemachineInfoList = new List<StateMachineInfo>(
                        stateMachines.Select(stateMachine => new StateMachineInfo
                            {
                                StateMachineName = stateMachine?.Attribute(XCApiTags.Name)?.Value,
                                StateMachineCode = Convert.ToInt32(stateMachine?.Attribute(XCApiTags.Id)?.Value)
                            })
                            .ToList()
                    );

                    if (statemachineInfoList.Any())
                    {
                        _stateMachineInfoByComponentRepository.Add(componentName, statemachineInfoList);
                    }
                }
            }
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
                    Convert.ToInt32(node.Attribute(XCApiTags.EventCode)?.Value));
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
                repository.Add(topicIdentifier, node?.Descendants(_xc + XCApiTags.Topic).FirstOrDefault()?.Value);
            }
        }

        private Dictionary<long, string> CreateSnapshotTopicByComponentRepository(IEnumerable<XElement> snapshotNodes)
        {
            var snapshotTopicByComponentRepo = new Dictionary<long, string>();
            foreach (var node in snapshotNodes)
            {
                AddSnapshotTopicToRepository(snapshotTopicByComponentRepo, node);
            }
            return snapshotTopicByComponentRepo;
        }

        private void AddSnapshotTopicToRepository(IDictionary<long, string> repository, XElement node)
        {
            var componentCode = Convert.ToInt64(node?.Attribute(XCApiTags.ComponentCode)?.Value);

            if (!repository.ContainsKey(componentCode))
            {
                repository.Add(componentCode, node?.Descendants(_xc + XCApiTags.Topic).FirstOrDefault()?.Value);
            }
        }

        public string GetConnectionType()
        {
            var commmunicationNode = _xcApiDescription.GetCommunicationNode()?.FirstOrDefault();

            var communicationChildElements = commmunicationNode?.Elements().ToList();

            if (communicationChildElements != null && communicationChildElements.Count == 1)
            {
                var connection = communicationChildElements.FirstOrDefault();
                return connection?.Name.LocalName;
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
            _componentCodeByComponentNameRepo.TryGetValue(component, out componentCode);
            return componentCode;
        }

        public int GetStateMachineCode(string component, string stateMachine)
        {
            if (!_stateMachineInfoByComponentRepository.ContainsKey(component)) return 0;

            var stateMachineInfo = _stateMachineInfoByComponentRepository[component]?.FirstOrDefault(stmInfo => stmInfo.StateMachineName.Equals(stateMachine));
            return stateMachineInfo?.StateMachineCode ?? 0;
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
