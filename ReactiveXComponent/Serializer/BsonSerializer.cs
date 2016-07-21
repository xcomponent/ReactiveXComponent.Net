using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace XComponent.Communication.Serialization
{
    public class BsonSerializer : ISerializer
    {
        public void Serialize(Stream stream, object message)
        {
            var bsonWriter = new BsonWriter(stream);
            var serializer = JsonSerializer.Create(PlainJsonSerializer.SerializerSettings);
            serializer.Converters.Add(new KindNeutralDateTimeConverter());
            serializer.Serialize(bsonWriter, message);
            bsonWriter.Flush();
        }

        public object Deserialize(Stream stream, string messageTypeName)
        {
            Type messageType = SerializerTypeCache.Instance.LookUpTypeName(messageTypeName);
            if (messageType == null)
            {
                throw new SerializationException(string.Format("Could not deserialize message type {0}. Type not found in cache", messageTypeName));
            }
            var bsonReader = new BsonReader(stream);
            var serializer = JsonSerializer.Create(PlainJsonSerializer.SerializerSettings);
            serializer.Converters.Add(new KindNeutralDateTimeConverter());
            return serializer.Deserialize(bsonReader, messageType);
        }

    }
}