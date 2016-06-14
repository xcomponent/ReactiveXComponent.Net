using System.Xml;

namespace ReactiveXComponent.Parser
{
    public class XDocumentConverter
    {
        private readonly XmlDocument _doc;
        private readonly XmlNamespaceManager _xmlManager;

        public XDocumentConverter(string document)
        {
            _doc = new XmlDocument();
            _doc.LoadXml(document.ToString());

            _xmlManager = new XmlNamespaceManager(_doc.NameTable);
            _xmlManager.AddNamespace("xc", "http://xcomponent.com/DeploymentConfig.xsd");
        }

        public XmlNodeList GetComponentsNode()
        {
            return _doc.SelectNodes("//xc:component", _xmlManager);
        }

        public XmlNodeList GetStateMachinesNode()
        {
            return _doc.SelectNodes("//xc:stateMachines", _xmlManager);
        }

        public XmlNodeList GetPublishersNode()
        {
            return _doc.SelectNodes("//xc:publish", _xmlManager);
        }

        public XmlNodeList GetConsumersNode()
        {
            return _doc.SelectNodes("//xc:subscribe[@eventType='UPDATE']", _xmlManager); ;
        }

    }
}
