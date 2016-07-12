using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ReactiveXComponent.Serializer
{
    public class JsonSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public void Serialize(Stream stream, object message)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(SerializerSettings);
            var streamWriter = new StreamWriter(stream);
            var jsonTextWriter = new JsonTextWriter(streamWriter);

            serializer.Serialize(jsonTextWriter, message);
            jsonTextWriter.Flush();
        }

        public object Deserialize(Stream stream)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(SerializerSettings);
            var streamReader = new StreamReader(stream);
            var jsonTextReader = new JsonTextReader(streamReader);

            return serializer.Deserialize(jsonTextReader);
        }
    }
}