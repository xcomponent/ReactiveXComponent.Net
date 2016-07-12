using System.IO;

namespace ReactiveXComponent.Serializer
{
    public interface ISerializer
    {
        object Deserialize(Stream stream);
        void Serialize(Stream stream, object message);
    }
}