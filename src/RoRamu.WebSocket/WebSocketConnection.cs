namespace RoRamu.WebSocket
{
    using System;
    using System.Threading.Tasks;

    public abstract class WebSocketConnection
    {
        public Action OnClose { get; set; }

        public Action<Exception> OnError { get; set; }

        public Action<string> OnMessage { get; set; }

        public abstract bool IsOpen { get; }

        public abstract Task SendMessage(string message);

        public abstract Task Close();
    }
}
