using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReactiveXComponent.Common
{
    public class GZipSnapshotItemArrayJsonConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonTextWriter.Formatting = serializer.Formatting;
                serializer.Serialize(jsonTextWriter, value);
                var input = Encoding.UTF8.GetBytes(stringWriter.ToString());
                using (var ms = new MemoryStream())
                {
                    using (var compressedStream = new GZipStream(ms, CompressionMode.Compress))
                    {
                        compressedStream.Write(input, 0, input.Length);
                        compressedStream.Close();
                    }
                    var compressedBase64 = Convert.ToBase64String(ms.ToArray());
                    serializer.Serialize(writer, compressedBase64, typeof(string));
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var input = reader.Value as string;
            if (input != null)
            {
                var compressedInput = Convert.FromBase64String(input);
                using (var ms = new MemoryStream(compressedInput))
                {
                    using (var decompressedStream = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        using (var streamReader = new StreamReader(decompressedStream))
                        {
                            return serializer.Deserialize(streamReader, typeof(T));
                        }
                    }
                }
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }
    }
}
