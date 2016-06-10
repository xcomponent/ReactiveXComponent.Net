using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ReactiveXComponent.Parser
{
    public class DeploymentParser
    {
        private readonly XDocument _document;
        
        public DeploymentParser(Stream file)
        {
            var reader = XmlReader.Create(file);
            _document = XDocument.Load(reader);
        }

        public long GetComponentCode(string componentName)
        {
            var componentCode = (from component in _document?.Descendants() where component?.Attribute("name")?.Value == componentName select component.Attribute("id").Value).FirstOrDefault();
            return Convert.ToInt64(componentCode);
        }

        public long GetStateMachineCode(string componentName, string stateMachineName)
        {
            var stateMachines = from component in _document?.Descendants() where component?.Attribute("name")?.Value == componentName select component;
            return Convert.ToInt64((from stateMachine in stateMachines.Descendants() where stateMachine?.Attribute("name")?.Value == stateMachineName select stateMachine?.Attribute("id")?.Value).FirstOrDefault());
        }

        public int GetPublisherEventCode(string message)
        {
            var eventCode = from publish in _document?.Descendants() where publish?.Name.LocalName == "publish" && publish?.Attribute("event")?.Value == message select publish.Attribute("eventCode");
            return Convert.ToInt32(eventCode.FirstOrDefault()?.Value);
        }

        public string GetPublisherTopic(string component, string stateMachine, int eventCode)
        {
            var publishTag = from publish in _document.Descendants() where publish?.Name.LocalName == "publish" && publish.Attribute("componentCode")?.Value == GetComponentCode(component).ToString() && publish.Attribute("stateMachineCode").Value == GetStateMachineCode(component, stateMachine).ToString() && publish.Attribute("eventCode").Value == eventCode.ToString() select publish;
            return (from topic in publishTag.FirstOrDefault()?.Descendants() where topic?.Name.LocalName == "topic" select topic.Value).FirstOrDefault();
        }

        public string GetConsumerTopic(string component, string stateMachine)
        {
            var consumerTag = from consumer in _document.Descendants() where consumer?.Name.LocalName == "subscribe" && consumer.Attribute("componentCode")?.Value == GetComponentCode(component).ToString() && consumer.Attribute("stateMachineCode")?.Value == GetStateMachineCode(component, stateMachine).ToString() && consumer.Attribute("eventType")?.Value == "UPDATE" select consumer;
            return (from topic in consumerTag.FirstOrDefault()?.Descendants() where topic?.Name.LocalName == "topic" select topic.Value).FirstOrDefault();
        }
    }
}
