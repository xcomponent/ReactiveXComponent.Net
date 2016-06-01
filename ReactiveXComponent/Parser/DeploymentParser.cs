using System.Linq;
using System.Xml.Linq;
using ReactiveXComponent.Common;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.Parser
{
    public class DeploymentParser
    {
        private XDocument _document;
        private BusDetails _busDetails;

        public DeploymentParser(string file)
        {
            _document = XDocument.Load(file);

            InitBusDetails();
        }

        public string Host
        {
            set { BusDetails.Host = value; }
            get { return BusDetails.Host; }
        }

        public int Port
        {
            set { BusDetails.Port = value; }
            get { return BusDetails.Port; }
        }

        public BusDetails BusDetails
        {
            get { return _busDetails; }
        }

        public void InitBusDetails()
        {
            if (BusDetails == null)
            {
                _busDetails =(from bus in this._document.Descendants() where bus.Name.LocalName == "bus" select new BusDetails() { Host = bus.Attribute("host").Value, Username = bus.Attribute("user").Value, Password = bus.Attribute("password").Value, Port = int.Parse(bus.Attribute("port").Value) }).FirstOrDefault();
            }
        }

        public string GetPublisherTopic(string component, string stateMachine, int eventCode)
        {
            var publishTag = from publish in this._document.Descendants() where publish.Name.LocalName == "publish" && publish.Attribute("componentCode").Value == HashcodeHelper.GetXcHashCode(component).ToString() && publish.Attribute("stateMachineCode").Value == HashcodeHelper.GetXcHashCode(stateMachine).ToString() && publish.Attribute("eventCode").Value == eventCode.ToString() select publish;
            return (from topic in publishTag.FirstOrDefault().Descendants() where topic.Name.LocalName == "topic" select topic.Value).FirstOrDefault();
        }

        public string GetConsumerTopic(string component, string stateMachine)
        {
            var consumerTag = from consumer in this._document.Descendants() where consumer.Name.LocalName == "subscribe" && consumer.Attribute("componentCode") != null && consumer.Attribute("componentCode").Value == HashcodeHelper.GetXcHashCode(component).ToString() && consumer.Attribute("stateMachineCode") != null && consumer.Attribute("stateMachineCode").Value == HashcodeHelper.GetXcHashCode(stateMachine).ToString() && consumer.Attribute("eventType") != null && consumer.Attribute("eventType").Value == "UPDATE" select consumer;
            return (from topic in consumerTag.FirstOrDefault().Descendants() where topic.Name.LocalName == "topic" select topic.Value).FirstOrDefault();
        }
    }
}
