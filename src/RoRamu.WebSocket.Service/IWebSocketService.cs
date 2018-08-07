namespace RoRamu.WebSocket.Service
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWebSocketService<TWebSocketProxy>
    {
        /// <summary>
        /// The currently active websocket connections.
        /// </summary>
        IReadOnlyDictionary<string, TWebSocketProxy> Connections { get; }

        /// <summary>
        /// Sends a message to all currently active websocket connections.
        /// </summary>
        /// <param name="message">The message to send</param>
        Task Broadcast(string message);
    }
}
