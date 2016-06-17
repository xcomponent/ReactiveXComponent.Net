using System.Xml;

namespace ReactiveXComponent.Parser
{
    public class XCApiDescription
    {
        private readonly XmlDocument _doc;
        private readonly XmlNamespaceManager _xmlManager;

        public XCApiDescription(XmlDocument document)
        {
            _doc = document;
            _xmlManager = new XmlNamespaceManager(_doc.NameTable);
            _xmlManager.AddNamespace("xc", "http://xcomponent.com/DeploymentConfig.xsd");
        }

        public XmlNodeList GetComponentsNode()
        {
            return _doc.SelectNodes("//xc:component", _xmlManager);
        }

        public XmlNodeList GetPublishersNode()
        {
            return _doc.SelectNodes("//xc:publish", _xmlManager);
        }

        public XmlNodeList GetConsumersNode()
        {
            return _doc.SelectNodes("//xc:subscribe[@eventType='UPDATE']", _xmlManager);
        }

        public XmlNodeList GetCommunicationNode()
        {
            return _doc.SelectNodes("//xc:communication", _xmlManager);
        }
    }
}
