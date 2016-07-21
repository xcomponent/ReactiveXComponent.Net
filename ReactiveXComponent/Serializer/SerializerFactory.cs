namespace ReactiveXComponent.Serializer
{
    public static class SerializerFactory
    {
        public static ISerializer CreateSerializer(SerializationType serializationType)
        {
            switch (serializationType)
            {
                case SerializationType.Binary:
                    return new BinarySerializer();
                case SerializationType.Json:
                    return new JsonSerializer();
                default:
                    throw new XCSerializationException("Unhandled serialization type " + serializationType);
            }
        }
    }
}