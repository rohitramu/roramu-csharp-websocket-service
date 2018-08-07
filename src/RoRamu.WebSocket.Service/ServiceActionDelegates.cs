namespace RoRamu.WebSocket.Service
{
    using System.Threading.Tasks;

    public delegate bool IsOpen();
    public delegate Task SendMessage(string message);
    public delegate Task Close();
}
