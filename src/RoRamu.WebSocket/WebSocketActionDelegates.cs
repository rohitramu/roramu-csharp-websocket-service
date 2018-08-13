namespace RoRamu.WebSocket
{
    using System.Threading.Tasks;

    public delegate bool IsOpenDelegate();
    public delegate Task SendMessageDelegate(Message message);
    public delegate Task CloseDelegate();
}
