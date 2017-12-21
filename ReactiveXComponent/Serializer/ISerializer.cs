using System.IO;

namespace ReactiveXComponent.Serializer
{
    public interface ISerializer
    {
        object Deserialize(Stream stream);
        T CastObject<T>(object message) where T : class;
        void Serialize(Stream stream, object message);
    }
}