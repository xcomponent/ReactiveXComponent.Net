using System;
using Newtonsoft.Json;
using XComponent.Common.Helper;
using XComponent.Communication.Serialization.Exceptions;

namespace XComponent.Communication.Serialization
{
    public class KindNeutralDateTimeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                DateTime dateTime = (DateTime)value;
                writer.WriteValue(dateTime.ToBinary());
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!ReflectionHelper.IsNullableType(objectType))
                    throw new XCSerializationException(string.Format("Cannot convert null value to {0}.", objectType));
                else
                    return null;
            }

            long value = (long)reader.Value;
            DateTime result = DateTime.FromBinary(value);
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }
    }
}
