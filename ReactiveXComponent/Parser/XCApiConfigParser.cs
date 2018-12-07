using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
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
        private Dictionary<string, int> _eventCodeByEventRepository;
        private Dictionary<TopicIdentifier, string> _publisherTopicByIdentifierRepository;
        private Dictionary<TopicIdentifier, string> _subscriberTopicByIdentifierRepository;
        private Dictionary<long, string> _snapshotTopicByComponentRepository;
        private readonly Dictionary<string, List<StateMachineInfo>> _stateMachineInfoByComponentRepository =
            new Dictionary<string, List<StateMachineInfo>>();
        private readonly Dictionary<string, int> _componentCodeByComponentNameRepository = new Dictionary<string, int>();


        public void Parse(Stream xcApiStream)
        {
            var reader = XmlReader.Create(xcApiStream);
            _xcApiDescription = new XCApiDescription(reader);
            _xc = "http://xcomponent.com/DeploymentConfig.xsd";

            ParseComponentNodes(_xcApiDescription.GetComponentsNode());
            _eventCodeByEventRepository = CreateEventCodeByEventRepository(_xcApiDescription.GetPublishersNode());
            _publisherTopicByIdentifierRepository =
                CreatePublisherTopicByComponentAndStateMachineRepository(_xcApiDescription.GetPublishersNode());
            _subscriberTopicByIdentifierRepository =
                CreateConsumerTopicByComponentAndStateMachineRepository(_xcApiDescription.GetConsumersNode());
            _snapshotTopicByComponentRepository = CreateSnapshotTopicByComponentRepository(_xcApiDescription.GetSnapshotsNode());
        }

        private void ParseComponentNodes(IEnumerable<XElement> components)
        {
            foreach (XElement component in components)
            {
                var componentName = component.Attribute(XCApiTags.Name)?.Value;
                var componentCode = component.Attribute(XCApiTags.Id)?.Value;

                if (!string.IsNullOrEmpty(componentName) && !string.IsNullOrEmpty(componentCode))
                {
                    _componentCodeByComponentNameRepository.Add(componentName, Convert.ToInt32(componentCode));

                    ParseStateMachineNodes(component, componentName);
                }
            }
        }

        private void ParseStateMachineNodes(XElement component, string componentName)
        {
            if (_stateMachineInfoByComponentRepository.ContainsKey(componentName)) return;

            var stateMachines = component.Descendants(_xc + XCApiTags.StateMachine);
            var statemachineInfoList = new List<StateMachineInfo>
                (
                    stateMachines
                        .Where(stateMachine => 
                                !string.IsNullOrEmpty(stateMachine?.Attribute(XCApiTags.Name)?.Value)
                                && !string.IsNullOrEmpty(stateMachine.Attribute(XCApiTags.Id)?.Value))
                        .Select(stateMachine =>
                            new StateMachineInfo(stateMachine.Attribute(XCApiTags.Name)?.Value, 
                            Convert.ToInt32(stateMachine.Attribute(XCApiTags.Id)?.Value))
                ).ToList()
            );

            if (statemachineInfoList.Any())
            {
                _stateMachineInfoByComponentRepository.Add(componentName, statemachineInfoList);
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
            var eventName = node?.Attribute(XCApiTags.EventName)?.Value;
            var eventCode = node?.Attribute(XCApiTags.EventCode)?.Value;

            if (!string.IsNullOrEmpty(eventName) 
                && !repository.ContainsKey(eventName)
                && !string.IsNullOrEmpty(eventCode))
            {
                repository.Add(eventName, Convert.ToInt32(eventCode));
            }
        }

        private Dictionary<TopicIdentifier, string> CreatePublisherTopicByComponentAndStateMachineRepository(
            IEnumerable<XElement> publishNodes)
        {
            Dictionary<TopicIdentifier, string> topicByIdentifierRepo = new Dictionary<TopicIdentifier, string>();

            foreach (XElement node in publishNodes)
            {
                AddTopicToRepository(topicByIdentifierRepo, node);
            }
            return topicByIdentifierRepo;
        }

        private Dictionary<TopicIdentifier, string> CreateConsumerTopicByComponentAndStateMachineRepository(
            IEnumerable<XElement> subscribeNodes)
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
            var topicValue = node?.Descendants(_xc + XCApiTags.Topic).FirstOrDefault()?.Value;
            var topicIdentifier = new TopicIdentifier(componentCode, stateMachineCode, topicType);
            
            if (!repository.ContainsKey(topicIdentifier) && !string.IsNullOrEmpty(topicValue))
            {
                repository.Add(topicIdentifier, topicValue);
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
            var snapshotTopic = node?.Descendants(_xc + XCApiTags.Topic).FirstOrDefault()?.Value;

            if (!repository.ContainsKey(componentCode) && !string.IsNullOrEmpty(snapshotTopic))
            {
                repository.Add(componentCode, snapshotTopic);
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
            return _xcApiDescription.GetSerializationNode()?.FirstOrDefault()?.Value;
        }

        public BusDetails GetBusDetails()
        {
            XElement busInfos = _xcApiDescription.GetBusNode()?.FirstOrDefault();

            var sslEnabledString = busInfos?.Attribute(XCApiTags.BusSslEnabled)?.Value;
            var sslEnabled = false;
            if (!string.IsNullOrEmpty(sslEnabledString))
            {
                bool.TryParse(sslEnabledString, out sslEnabled);
            }

            var sslServerName = busInfos?.Attribute(XCApiTags.BusSslServerName)?.Value;
            var sslCertificatePath = busInfos?.Attribute(XCApiTags.BusSslCertificatePath)?.Value;
            var sslCertificatePassphrase = busInfos?.Attribute(XCApiTags.BusSslCertificatePassphrase)?.Value;

            var sslProtocolString = busInfos?.Attribute(XCApiTags.BusSslProtocol)?.Value;
            SslProtocols sslProtocol = SslProtocols.Default;
            if (!string.IsNullOrEmpty(sslProtocolString))
            {
                Enum.TryParse(sslProtocolString, out sslProtocol);
            }

            var sslAllowUntrustedServerCertificateString = busInfos?.Attribute(XCApiTags.BusSslAllowUntrustedServerCertificate)?.Value;
            var sslAllowUntrustedServerCertificate = false;
            if (!string.IsNullOrEmpty(sslAllowUntrustedServerCertificateString))
            {
                bool.TryParse(sslAllowUntrustedServerCertificateString, out sslAllowUntrustedServerCertificate);
            }

            var busDetails = new BusDetails(
                busInfos?.Attribute(XCApiTags.User)?.Value,
                busInfos?.Attribute(XCApiTags.Password)?.Value,
                busInfos?.Attribute(XCApiTags.Host)?.Value,
                busInfos?.Attribute(XCApiTags.VirtualHost)?.Value,
                Convert.ToInt32(busInfos?.Attribute(XCApiTags.Port)?.Value),
                sslEnabled,
                sslServerName,
                sslCertificatePath,
                sslCertificatePassphrase,
                sslProtocol,
                sslAllowUntrustedServerCertificate);

            return busDetails;
        }

        public WebSocketEndpoint GetWebSocketEndpoint()
        {
            XElement websocketInfos = _xcApiDescription.GetWebSocketNode()?.FirstOrDefault();

            WebSocketType webSocketType;
            var webSocketTypeString = websocketInfos?.Attribute(XCApiTags.WebsocketType)?.Value;
            if (!Enum.TryParse(webSocketTypeString, out webSocketType))
            {
                throw new ReactiveXComponentException($"Could not parse communication type: {webSocketTypeString}");
            }

            var webSocketEndpoint = new WebSocketEndpoint(
                websocketInfos?.Attribute(XCApiTags.Name)?.Value,
                websocketInfos?.Attribute(XCApiTags.Host)?.Value,
                websocketInfos?.Attribute(XCApiTags.Port)?.Value,
                webSocketType);

            return webSocketEndpoint;
        }

        public int GetComponentCode(string component)
        {
            int componentCode;
            _componentCodeByComponentNameRepository.TryGetValue(component, out componentCode);
            return componentCode;
        }

        public int GetStateMachineCode(string component, string stateMachine)
        {
            if (!_stateMachineInfoByComponentRepository.ContainsKey(component)) return 0;

            var stateMachineInfo = _stateMachineInfoByComponentRepository[component]
                ?.FirstOrDefault(stmInfo => stmInfo.StateMachineName.Equals(stateMachine));
            return stateMachineInfo?.StateMachineCode ?? 0;
        }

        public int GetPublisherEventCode(string eventName)
        {
            int eventCode;
            _eventCodeByEventRepository.TryGetValue(eventName, out eventCode);
            return eventCode;
        }

        public string GetPublisherTopic(string component, string stateMachine)
        {
            string publisherTopic;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = GetStateMachineCode(component, stateMachine);
            var topicId = new TopicIdentifier(componentCode, stateMachineCode, XCApiTags.Output);

            _publisherTopicByIdentifierRepository.TryGetValue(topicId, out publisherTopic);
            return publisherTopic;
        }

        public string GetPublisherTopic(long componentCode, long stateMachineCode)
        {
            string publisherTopic;
            var topicId = new TopicIdentifier(componentCode, stateMachineCode, XCApiTags.Output);

            _publisherTopicByIdentifierRepository.TryGetValue(topicId, out publisherTopic);

            return publisherTopic;
        }

        public string GetSubscriberTopic(string component, string stateMachine)
        {
            string subscriberTopic;
            var componentCode = GetComponentCode(component);
            var stateMachineCode = GetStateMachineCode(component, stateMachine);
            var topicId = new TopicIdentifier(componentCode, stateMachineCode, XCApiTags.Input);

            _subscriberTopicByIdentifierRepository.TryGetValue(topicId, out subscriberTopic);
            return subscriberTopic;
        }

        public string GetSnapshotTopic(string component)
        {
            string snapshotTopic;
            var componentCode = GetComponentCode(component);
            _snapshotTopicByComponentRepository.TryGetValue(componentCode, out snapshotTopic);
            return snapshotTopic;
        }
    }
}