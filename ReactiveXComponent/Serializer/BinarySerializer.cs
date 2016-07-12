using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ReactiveXComponent.Serializer
{
    public class BinarySerializer : ISerializer
    {

        public void Serialize(Stream stream, object message)
        {
            if (message == null) return;
            var binaryFormater = new BinaryFormatter();
            try
            {
                binaryFormater.Serialize(stream, message);
            }
            catch (Exception ex)
            {
                throw new RXCSerializationException(ex.Message, ex);
            }
        }

        public object Deserialize(Stream stream)
        {
            object obj;
            if (stream == null) return null;
            var binaryFormater = new BinaryFormatter();                
            try
            {
                obj = binaryFormater.Deserialize(stream);
            }
            catch (Exception ex)
            {
                throw new SerializationException(ex.Message, ex);
            }

            return obj;
        }
    }
}