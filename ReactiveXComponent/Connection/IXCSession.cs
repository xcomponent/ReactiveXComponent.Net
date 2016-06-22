namespace ReactiveXComponent.Connection
{
    public interface IXCSession
    {
        IXCPublisher CreatePublisher(string component);
    }
}