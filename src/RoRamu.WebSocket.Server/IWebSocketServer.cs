namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Threading.Tasks;

    public interface IWebSocketServer
    {
        Action<WebSocketConnection, WebSocketConnectionInfo> OnOpen { get; set; }

        Task Start();

        Task Stop();
    }
}
