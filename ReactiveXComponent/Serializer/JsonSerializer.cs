using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            try
            {
                serializer.Serialize(jsonTextWriter, message);
                jsonTextWriter.Flush();
            }
            catch (Exception e)
            {
                throw new XCSerializationException(e.Message, e);
            }
        }

        public object Deserialize(Stream stream)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(SerializerSettings);
            var streamReader = new StreamReader(stream);
            var jsonTextReader = new JsonTextReader(streamReader);

            try
            {
                return serializer.Deserialize(jsonTextReader);
            }
            catch (Exception e)
            {
                throw new XCSerializationException(e.Message, e);
            }
        }

        public T CastObject<T>(object message) where T : class
        {
            var jResult = message as JObject;
            return jResult?.ToObject<T>();
        }
    }
}