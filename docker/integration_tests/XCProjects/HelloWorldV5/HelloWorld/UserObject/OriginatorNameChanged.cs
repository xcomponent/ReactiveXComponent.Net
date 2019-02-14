using XComponent.Shared.Failover;

namespace XComponent.HelloWorld.UserObject
{
    public class OriginatorNameChanged
    {
        public string OriginatorName { get; }

        public OriginatorNameChanged(string originatorName)
        {
            OriginatorName = originatorName;
        }
    }
}
