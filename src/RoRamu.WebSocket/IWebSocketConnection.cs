namespace RoRamu.WebSocket
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.Utils.Messaging;

    /// <summary>
    /// Represents the actions available for interacting with a websocket connection.
    /// </summary>
    public interface IWebSocketConnection
    {
        /// <summary>
        /// The logger to use.
        /// </summary>
        Logger Logger { get; set; }

        /// <summary>
        /// The timeout for requests.
        /// </summary>
        TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        Task SendMessage(Message message);

        /// <summary>
        /// Sends a request and then waits for the given timeout for a response.  If the timeout is hit, this method will throw a <see cref="TimeoutException"/>.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="requestTimeout">The timeout to use for this request - leave as null to use the default timeout (<see cref="WebSocketConnectionProxy.RequestTimeout"/>).</param>
        /// <returns>The response.</returns>
        Task<RequestResult> SendRequest(Request request, TimeSpan? requestTimeout = null);

        /// <summary>
        /// Whether or not the underlying connection is open.
        /// </summary>
        /// <returns>True if the connection is open, otherwise false.</returns>
        bool IsOpen();

        /// <summary>
        /// Closes this connection.
        /// </summary>
        Task Close();
    }
}