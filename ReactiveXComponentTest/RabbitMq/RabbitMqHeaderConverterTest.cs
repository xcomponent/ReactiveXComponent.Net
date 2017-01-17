﻿using System.Text;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.RabbitMq
{
    [TestFixture]
    public class RabbitMqHeaderConverterTest
    {
        private Header _header;
        private readonly UTF8Encoding _encoding = new UTF8Encoding();

        [SetUp]
        public void Setup()
        {
            const long stateMachineCode = 1183299;
            const long componentCode = 1183300;
            const int eventCode = 153;
            const string Event = "GetSessionRequest";
            const string visibility = "Public";

            _header = new Header()
            {
                StateMachineCode = stateMachineCode,
                ComponentCode = componentCode,
                EventCode = eventCode,
                MessageType = Event,
                PublishTopic = visibility
            };
        }

        [Test]
        public void ConvertHeader_GivenHeader_ShouldConvertHeaderToADico_Test()
        {
            var headerDico = RabbitMqHeaderConverter.ConvertHeader(_header);

            Check.That(headerDico.Values).Contains(_header.ComponentCode).And.Contains(_header.StateMachineCode).And.Contains(_header.EventCode);
            Assert.AreEqual(headerDico["PrivateTopic"], _encoding.GetBytes(_header.PublishTopic));
            Assert.AreEqual(headerDico["MessageType"], _encoding.GetBytes(_header.MessageType));
        }

        [Test]
        public void ConvertHeader_GivenADico_ShouldConvertDicoToAHeader_Test()
        {
            var headerDico = RabbitMqHeaderConverter.ConvertHeader(_header);
            var headerExpected = _header;        
            var stateMachineRef = RabbitMqHeaderConverter.ConvertStateMachineRefHeader(headerDico);
            
            Assert.IsTrue(ConatainsHeader(headerExpected, stateMachineRef));
        }

        private bool ConatainsHeader(Header header, StateMachineRefHeader stateMachineRef)
        {
            return stateMachineRef.StateMachineCode == header.StateMachineCode &&
                    stateMachineRef.ComponentCode == header.ComponentCode &&
                    stateMachineRef.EventCode == header.EventCode &&
                    stateMachineRef.MessageType == header.MessageType &&
                    stateMachineRef.PublishTopic == header.PublishTopic;
        }

        [TearDown]
        public void TearDown()
        {
            _header = null;
        }
    }
}