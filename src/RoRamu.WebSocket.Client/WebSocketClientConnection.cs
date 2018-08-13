namespace RoRamu.WebSocket.Client
{
    using System;
    using System.Threading.Tasks;

    public abstract class WebSocketClientConnection : WebSocketConnection
    {
        public Action OnOpen { get; set; }

        public abstract Task Connect();
    }
}
