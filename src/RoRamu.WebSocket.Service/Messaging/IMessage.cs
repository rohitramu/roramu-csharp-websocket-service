namespace RoRamu.WebSocket.Service
{
    public interface IMessage
    {
        string MessageType { get; }

        string Body { get; }
    }
}
