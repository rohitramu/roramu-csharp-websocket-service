namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Threading.Tasks;

    public interface IWebSocketServer
    {
        Action<WebSocket> OnOpen { get; set; }

        Task Start();

        void Stop();
    }
}
