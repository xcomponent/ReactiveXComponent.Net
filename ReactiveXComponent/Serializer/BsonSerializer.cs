﻿using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace ReactiveXComponent.Serializer
{
    public class BsonSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer = new Newtonsoft.Json.JsonSerializer();

        public void Serialize(Stream stream, object value)
        {
            var bsonWriter = new BsonWriter(stream);
            _serializer.Serialize(bsonWriter, value);
            bsonWriter.Flush();
        }

        public object Deserialize(Stream stream)
        {
            var bsonReader = new BsonReader(stream);
            return _serializer.Deserialize(bsonReader);
        }

        public T CastObject<T>(object message) where T : class
        {
            var jResult = message as JObject;
            return jResult?.ToObject<T>();
        }
    }
}