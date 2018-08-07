namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWebSocket
    {
        Action OnClose { get; set; }
        Action<Exception> OnError { get; set; }
        Action<string> OnMessage { get; set; }

        bool IsOpen { get; }

        string ClientIpAddress { get; }

        IReadOnlyDictionary<string, string> Headers { get; }

        IReadOnlyDictionary<string, string> Cookies { get; }

        Task SendMessage(string message);

        Task Close();
    }
}
