using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponentTest.Serializer
{
    internal class TestMessage
    {
        public TestMessage(string data, string extraData)
        {
            Data = data;
            ExtraData = extraData;
        }

        public string Data { get; }
        public string ExtraData { get; }
    }

    [TestFixture]
    public class SerializationTests
    {
        [TestCase(SerializationType.Binary)]
        [TestCase(SerializationType.Json)]
        [TestCase(SerializationType.Bson)]
        public void SerializerFactoryTest(SerializationType serializationType)
        {
            try
            {
                var serializer = SerializerFactory.CreateSerializer(serializationType);

                switch (serializationType)
                {
                    case SerializationType.Binary:
                    Check.That(serializer).IsInstanceOf<BinarySerializer>();
                    break;
                    case SerializationType.Json:
                    Check.That(serializer).IsInstanceOf<JsonSerializer>();
                    break;
                    case SerializationType.Bson:
                    Check.That(serializer).IsInstanceOf<BsonSerializer>();
                    break;
                }
            }
            catch (XCSerializationException e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void BinarySerializerTest()
        {
            var replyTopic = new ReplyTopic(){ Case = "some value" };
            var serializer = SerializerFactory.CreateSerializer(SerializationType.Binary);

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, replyTopic);
                stream.Position = 0;

                var deserializedReplyTopic = serializer.Deserialize(stream) as ReplyTopic;
                Check.That(deserializedReplyTopic).IsNotNull();
                Check.That(deserializedReplyTopic?.Case == replyTopic.Case);
            }
        }

        [Test]
        public void JsonSerializerTest()
        {
            var header = new WebSocketEngineHeader(){StateMachineCode = new Option<long>(405360011) };
            var serializer = SerializerFactory.CreateSerializer(SerializationType.Json);

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, header);
                stream.Position = 0;
                using (var streamReader = new StreamReader(stream))
                {
                    var serializedMessage = streamReader.ReadToEnd();
                    Check.That(serializedMessage).IsNotNull();
                    Check.That(serializedMessage).IsNotEmpty();
                    Check.That(serializedMessage).IsEqualTo("{\"StateMachineCode\":{\"Case\":\"Some\",\"Fields\":[405360011]},\"EventCode\":0,\"IncomingType\":0}");

                    using (var deserializationStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedMessage)))
                    {
                        var jObject = serializer.Deserialize(deserializationStream) as JObject;
                        var deserializedHeader = jObject?.ToObject<WebSocketEngineHeader>();
                        Check.That(deserializedHeader).IsNotNull();
                        Check.That(deserializedHeader?.StateMachineCode == header.StateMachineCode);
                    }
                }
            }
        }

        [Test]
        public void BsonSerializerTest()
        {
            var replyTopic = new ReplyTopic() { Case = "some value" };
            var serializer = SerializerFactory.CreateSerializer(SerializationType.Bson);

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, replyTopic);
                stream.Position = 0;

                var jObject = serializer.Deserialize(stream) as JObject;
                var deserializedReplyTopic = jObject?.ToObject<ReplyTopic>();
                Check.That(deserializedReplyTopic).IsNotNull();
                Check.That(deserializedReplyTopic?.Case == replyTopic.Case);
            }
        }
    }
}
