namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a websocket server, which hosts a websocket service.  Implementations of this
    /// interface implement the websocket protocol.
    /// </summary>
    public interface IWebSocketServer
    {
        /// <summary>
        /// A callback for when a connection is being made by a client.
        /// </summary>
        /// <value>
        /// The method to be called when a connection is being made by a client.
        /// </value>
        Action<WebSocketUnderlyingConnection, WebSocketConnectionInfo> OnOpen { get; set; }

        /// <summary>
        /// Starts the websocket server.
        /// Once the task completes, the server should be running (i.e. listening for new websocket
        /// connections on a port).
        /// </summary>
        Task Start();

        /// <summary>
        /// Stops the websocket server.
        /// Once the task completes, the server should be stopped.
        /// </summary>
        Task Stop();
    }
}
