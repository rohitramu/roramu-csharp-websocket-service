namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class WebSocket
    {
        public Action OnClose { get; set; }

        public Action<Exception> OnError { get; set; }

        public Action<string> OnMessage { get; set; }

        public string ClientIpAddress { get; protected set; }

        public IReadOnlyDictionary<string, string> Headers { get; protected set; }

        public IReadOnlyDictionary<string, string> Cookies { get; protected set; }

        public abstract bool IsOpen();

        public abstract Task SendMessage(string message);

        public abstract Task Close();
    }
}
