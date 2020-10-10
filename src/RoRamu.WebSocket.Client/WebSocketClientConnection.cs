namespace RoRamu.WebSocket.Client
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the underlying websocket client implementation's connection object.
    /// </summary>
    public abstract class WebSocketClientConnection : WebSocketUnderlyingConnection
    {
        /// <summary>
        /// A callback for when the connection is opened.
        /// </summary>
        /// <value>The method to be called when the connection is opened.</value>
        public Action OnOpen { get; set; }

        /// <summary>
        /// Connects to the websocket server.
        /// </summary>
        public abstract Task Connect();
    }
}
