namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Threading.Tasks;

    public interface IWebSocketProxy
    {
        string Id { get; }

        void OnOpen();

        void OnClose();

        void OnError(Exception ex);

        void OnMessage(string message);

        Task SendMessage(string message);

        Task Close();
    }
}
