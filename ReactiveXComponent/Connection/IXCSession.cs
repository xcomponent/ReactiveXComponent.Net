namespace ReactiveXComponent.Connection
{
    public interface IXCSession
    {
        IXCPublisher CreatePublisher(string component);
        IXCSubscriber CreateSubscriber(string component);
    }
}