namespace RoRamu.WebSocket.Service
{
    public interface IRequest : IMessage
    {
        string Id { get; }
    }
}
