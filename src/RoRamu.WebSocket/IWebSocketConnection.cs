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
        /// <value></value>
        TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        Task SendMessage(Message message);

        /// <summary>
        /// Sends a request and waits for the response.
        /// </summary>
        /// <param name="request">The request message to send.</param>
        /// <param name="requestTimeout">How long to wait for a response before timing out.</param>
        /// <returns>The result of the operation.</returns>
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