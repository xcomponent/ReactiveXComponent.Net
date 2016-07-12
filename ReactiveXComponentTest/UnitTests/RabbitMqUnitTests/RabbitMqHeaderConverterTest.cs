using System.Text;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.UnitTests.RabbitMqUnitTests
{
    [TestFixture]
    public class RabbitMqHeaderConverterTest
    {
        private SatetMachineRef _satetMachineRef;
        private readonly UnicodeEncoding _encoding = new UnicodeEncoding();

        [SetUp]
        public void Setup()
        {
            const long stateMachineCode = 1183299;
            const long componentCode = 1183300;
            const int eventCode = 153;
            const string Event = "GetSessionRequest";
            const string visibility = "Public";

            _satetMachineRef = new SatetMachineRef()
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
            var headerDico = RabbitMqHeaderConverter.ConvertHeader(_satetMachineRef);

            Check.That(headerDico.Values).Contains(_satetMachineRef.ComponentCode).And.Contains(_satetMachineRef.StateMachineCode).And.Contains(_satetMachineRef.EventCode);
            Assert.AreEqual(headerDico["PublishTopic"], _encoding.GetBytes(_satetMachineRef.PublishTopic));
            Assert.AreEqual(headerDico["MessageType"], _encoding.GetBytes(_satetMachineRef.MessageType));
        }

        [Test]
        public void ConvertHeader_GivenADico_ShouldConvertDicoToAHeader_Test()
        {
            var headerDico = RabbitMqHeaderConverter.ConvertHeader(_satetMachineRef);
            var headerExpected = _satetMachineRef;        
            var headerValue = RabbitMqHeaderConverter.ConvertHeader(headerDico);
            
            Assert.AreEqual(headerExpected, headerValue);
        }

        [TearDown]
        public void TearDown()
        {
            _satetMachineRef = null;
        }
    }
}
