using System;
using System.Collections.Generic;
using System.Text;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest.RabbitMqTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class HeaderConverterTest
    {
        private Header _header;
        private readonly UnicodeEncoding _encoding = new UnicodeEncoding();

        [SetUp]
        public void Setup()
        {
            const long userSession = 1183299321;
            const long userManagement = 1183299321;
            const long eventCode = 153;
            const long engineCode = -1805283946;
            const string Event = "XComponent.UserManagement.UserObject.GetSessionRequest";
            const string visibility = "Public";

            _header = new Header()
            {
                StateMachineCode = userSession,
                ComponentCode = userManagement,
                EventCode = eventCode,
                EngineCode = engineCode,
                MessageType = Event,
                PublishTopic = visibility
            };
        }

        [TestCase()]
        public void ConvertHeader_GivenHeader_ShouldConvertHeaderToADico_Test()
        {
            var headerDico = RabbitMqHeaderConverter.ConvertHeader(_header);

            Check.That(headerDico.Values).Contains(_header.ComponentCode).And.Contains(_header.StateMachineCode).And.Contains(_header.EventCode).And.Contains(_header.EngineCode);
            Assert.AreEqual(headerDico["PublishTopic"], _encoding.GetBytes(_header.PublishTopic));
            Assert.AreEqual(headerDico["MessageType"], _encoding.GetBytes(_header.MessageType));
        }

        [TestCase()]
        public void ConvertHeader_GivenADico_ShouldConvertDicoToAHeader_Test()
        {
            var headerDico = RabbitMqHeaderConverter.ConvertHeader(_header);
            var headerExpected = _header;        
            var headerValue = RabbitMqHeaderConverter.ConvertHeader(headerDico);
            
            Assert.AreEqual(headerExpected, headerValue);
        }
    }
}
