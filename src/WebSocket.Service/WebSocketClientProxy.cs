namespace RoRamu.WebSocket.Service
{
    using System;

    /// <summary>
    /// Represents the connection to a client.
    /// </summary>
    public sealed class WebSocketClientProxy : WebSocketConnectionProxy
    {
        /// <summary>
        /// The connection ID.
        /// </summary>
        public string Id { get; }

        internal WebSocketClientProxy(
            string id,
            WebSocketUnderlyingConnection connection,
            WebSocketController.FactoryDelegate controllerFactory)
            : base(connection, controllerFactory)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
        }
    }
}
