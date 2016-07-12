using System.Runtime.Serialization;

namespace ReactiveXComponent.Serializer
{
    public class SerializerFactory
    {
        private readonly SerializationType _serializationType;

        public SerializerFactory(SerializationType serializationType)
        {
            _serializationType = serializationType;
        }

        public ISerializer CreateSerializer()
        {
            switch (_serializationType)
            {
                case SerializationType.Binary:
                    return new BinarySerializer();
                case SerializationType.Json:
                    return new JsonSerializer();
                default:
                    throw new System.Runtime.Serialization.SerializationException("Unhandled serialization type " + _serializationType);
            }
        }
    }
}