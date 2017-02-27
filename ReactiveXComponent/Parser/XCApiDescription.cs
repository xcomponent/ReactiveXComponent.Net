using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace ReactiveXComponent.Parser
{
    public class XCApiDescription
    {
        private readonly XDocument _doc;
        private readonly XNamespace _xc;

        public XCApiDescription(XmlReader xmlReader)
        {
            _doc = XDocument.Load(xmlReader);
            _xc = "http://xcomponent.com/DeploymentConfig.xsd";
        }

        public IEnumerable<XElement> GetComponentsNode()
        {
            return _doc.Descendants(_xc + "component");
        }

        public IEnumerable<XElement> GetStateMachinesNode()
        {
            return _doc.Descendants(_xc + "stateMachine");
        }

        public IEnumerable<XElement> GetPublishersNode()
        {
            return _doc.Descendants(_xc + "publish");
        }

        public IEnumerable<XElement> GetConsumersNode()
        {
            return _doc.Descendants(_xc + "subscribe");
        }

        public IEnumerable<XElement> GetCommunicationNode()
        {
            return _doc.Descendants(_xc + "communication");
        }

        public IEnumerable<XElement> GetSerializationNode()
        {
            return _doc.Descendants(_xc + "serialization");
        }

        public IEnumerable<XElement> GetSnapshotsNode()
        {
            return _doc.Descendants(_xc + "snapshot");
        }

        public IEnumerable<XElement> GetBusNode()
        {
            return _doc.Descendants(_xc + "bus");
        }

        public IEnumerable<XElement> GetWebSocketNode()
        {
            return _doc.Descendants(_xc + "websocket");
        }
    }
}
