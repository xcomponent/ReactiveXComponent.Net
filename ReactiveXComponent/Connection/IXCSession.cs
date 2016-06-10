namespace ReactiveXComponent.Connection
{
    public interface IXCSession
    {
        IXCPublisher CreatePublisher();
        void InitPrivateCommunicationIdentifier(string privateCommunicationIdentifier);
    }
}