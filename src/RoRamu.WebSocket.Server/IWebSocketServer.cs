namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;

    public interface IWebSocketServer
    {
        Action<IWebSocket> OnOpen { get; set; }

        Logger Logger { get; }

        Task Start();

        void Stop();
    }
}
