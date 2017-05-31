using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Common;
using ReactiveXComponent.Serializer;

namespace ReactiveXComponentTest.Common
{
    [TestFixture]
    public class MessageEventArgsCastTests
    {
        [TestCase(SerializationType.Binary)]
        [TestCase(SerializationType.Json)]
        [TestCase(SerializationType.Bson)]
        public void GetMessageTest(SerializationType serializationType)
        {
            var testObject = new TestObject()
            {
                Id = 101,
                Name = "Test"
            };

            var serializer = SerializerFactory.CreateSerializer(serializationType);
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, testObject);
                stream.Position = 0;

                var deserializedObject = serializer.Deserialize(stream);
                var messageEventArgs = new MessageEventArgs(null, deserializedObject);
                var resultObject = messageEventArgs.GetMessage<TestObject>(serializationType);
                Check.That(resultObject).IsNotNull();
                Check.That(resultObject).IsEqualTo(testObject);
            }
        }
    }
}
