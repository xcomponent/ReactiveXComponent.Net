using XComponent.Shared.Failover;

namespace XComponent.HelloWorld.UserObject
{
    public class PingCountModified
    {
        public PingCountModified(int pingCount)
        {
            PingCount = pingCount;
        }

        public int PingCount { get; }
    }
}