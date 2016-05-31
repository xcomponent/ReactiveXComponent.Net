using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest
{
    [TestFixture]
    class HeaderConverterTest
    {
        [TestCase()]
        public void ConvertHeaderToHeaderDico_Sucess_Test()
        {
            var encoding = new UnicodeEncoding();
            const long userManagement = 1183299321;
            const long userSession = 1183299321;
            const long eventCode = 153;
            const long engineCode = -1805283946;
            const string Event = "XComponent.UserManagement.UserObject.GetSessionRequest";
            const string visibility = "Public";

            var header = new Header()
            {
                StateMachineCode = userSession,
                ComponentCode = userManagement,
                EventCode = eventCode,
                EngineCode = engineCode,
                MessageType = Event,
                PublishTopic = visibility
            };

            Dictionary<string, object> headerDico = RabbitMqHeaderConverter.ConvertHeader(header);

            Assert.IsTrue(headerDico.ContainsValue(header.StateMachineCode));
            Assert.AreEqual(headerDico["MessageType"], encoding.GetBytes(header.MessageType));

        }

        [TestCase()]
        public void ConvertHeaderDicoToHeader_Sucess_Test()
        {
            const long userManagement = 1183299321;
            const long userSession = 1183299321;
            const long eventCode = 153;
            const long engineCode = -1805283946;
            const string Event = "XComponent.UserManagement.UserObject.GetSessionRequest";
            const string visibility = "Public";
        
            var header = new Header()
            {
                StateMachineCode = userSession,
                ComponentCode = userManagement,
                EventCode = eventCode,
                EngineCode = engineCode,
                MessageType = Event,
                PublishTopic = visibility
            };
        
            Dictionary<string, object> headerDico = RabbitMqHeaderConverter.ConvertHeader(header);        
            Header headerValue = RabbitMqHeaderConverter.ConvertHeader(headerDico);
            
            Assert.AreEqual(header, headerValue);
        }
    }
}
