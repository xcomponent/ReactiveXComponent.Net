using System;
using System.Linq;
using System.Reflection;

namespace ReactiveXComponent.Serializer
{
    public static class SerializerFactory
    {
        private static ISerializer _customSerializer;
        private static readonly object CustomSerializerLock = new object();

        public static ISerializer CreateSerializer(SerializationType serializationType)
        {
            switch (serializationType)
            {
                case SerializationType.Binary:
                    return new BinarySerializer();
                case SerializationType.Json:
                    return new JsonSerializer();
                case SerializationType.Bson:
                    return new BsonSerializer();
                case SerializationType.Custom:
                    return FindCustomSerializer();
                default:
                    throw new XCSerializationException("Unhandled serialization type " + serializationType);
            }
        }

        private static ISerializer FindCustomSerializer()
        {
            if (_customSerializer == null)
            {
                lock (CustomSerializerLock)
                {
                    if (_customSerializer == null)
                    {
                        ISerializer customSerializer = null;

                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.GetCustomAttributes().OfType<CustomSerializerContainerAttribute>().Any())
                            {
                                foreach (var exportedType in assembly.GetExportedTypes())
                                {
                                    var customSerializerAttribute = exportedType.GetCustomAttributes()
                                        .OfType<CustomSerializerAttribute>()
                                        .FirstOrDefault();

                                    if (customSerializerAttribute != null)
                                    {
                                        customSerializer = (ISerializer)Activator.CreateInstance(exportedType);
                                        break;
                                    }
                                }
                            }
                        }

                        if (customSerializer == null)
                        {
                            throw new XCSerializationException("Unable to find a custom serializer");
                        }

                        _customSerializer = customSerializer;
                    }
                }
            }

            return _customSerializer;
        }
    }
}