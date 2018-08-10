namespace RoRamu.WebSocket.Service
{
    using System.Threading.Tasks;

    public delegate bool IsOpenDelegate();
    public delegate Task SendMessageDelegate(Message message);
    public delegate Task CloseDelegate();
}
