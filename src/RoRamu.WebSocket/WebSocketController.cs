namespace RoRamu.WebSocket
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Messaging;

    /// <summary>
    /// The logic for interacting with a websocket connection.
    /// </summary>
    public abstract class WebSocketController
    {
        /// <summary>
        /// A delegate representing a factory method for creating a websocket controller.
        /// </summary>
        /// <param name="connection">
        /// The actions available for interacting with a websocket connection.
        /// </param>
        /// <returns>A websocket controller.</returns>
        public delegate WebSocketController FactoryDelegate(IWebSocketConnection connection);

        /// <summary>
        /// The actions available for interacting with a given websocket connection.
        /// </summary>
        /// <value></value>
        protected IWebSocketConnection Connection { get; }

        /// <summary>
        /// Creates a new instance of this controller.
        /// </summary>
        /// <param name="webSocketConnection">
        /// The actions available for interacting with a given websocket connection.
        /// </param>
        public WebSocketController(IWebSocketConnection webSocketConnection)
        {
            this.Connection = webSocketConnection ?? throw new ArgumentNullException(nameof(webSocketConnection));
        }

        /// <summary>
        /// The callback for when the connection is opened.
        /// </summary>
        public virtual Task OnOpen() => Task.CompletedTask;

        /// <summary>
        /// The callback for when the connection is closed.
        /// </summary>
        public virtual Task OnClose() => Task.CompletedTask;

        /// <summary>
        /// The callback for when there is an error in the websocket connection.  The
        /// connection will most likely be already closed or closing soon.
        /// </summary>
        /// <param name="error">The exception which was thrown.</param>
        public virtual Task OnError(Exception error) => Task.CompletedTask;

        /// <summary>
        /// The callback for when a message is received on the connection.
        /// </summary>
        /// <param name="message">The message which was received.</param>
        public abstract Task OnMessage(Message message);
    }
}