namespace RoRamu.WebSocket.Service
{
    using System;

    /// <summary>
    /// The behavior for interacting with a connection to this service.
    /// </summary>
    public abstract class WebSocketServiceController : WebSocketController
    {
        /// <summary>
        /// The ID of connection on which this controller instance operates.
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// Creates a new websocket service controller.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="webSocketConnection">
        /// The actions available for interacting with this websocket connection.
        /// </param>
        /// <returns></returns>
        public WebSocketServiceController(string connectionId, IWebSocketConnection webSocketConnection) : base(webSocketConnection)
        {
            this.ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
        }
    }
}