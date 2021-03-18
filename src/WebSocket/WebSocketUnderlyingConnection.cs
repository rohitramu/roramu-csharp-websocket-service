namespace RoRamu.WebSocket
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// A wrapper for underlying implementations of websocket connections.
    /// </summary>
    public abstract class WebSocketUnderlyingConnection
    {
        /// <summary>
        /// The callback for when the connection is closed.
        /// </summary>
        public Func<Task> OnClose { get; internal set; }

        /// <summary>
        /// The callback for when there is an error in the websocket connection.  The
        /// connection will most likely be already closed or closing soon.
        /// </summary>
        public Func<Exception, Task> OnError { get; internal set; }

        /// <summary>
        /// The callback for when a message is received on the connection.
        /// </summary>
        public Func<string, Task> OnMessage { get; internal set; }

        /// <summary>
        /// Whether or not the connection is open.
        /// </summary>
        /// <returns>True if the connection is open, otherwise false.</returns>
        public abstract bool IsOpen();

        /// <summary>
        /// Sends a message over the connection.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public abstract Task SendMessage(string message);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public abstract Task Close();
    }
}
