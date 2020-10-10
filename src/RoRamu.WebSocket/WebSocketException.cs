namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception which related to a failure from the <c>"RoRamu.WebSocket"</c> library.
    /// </summary>
    public class WebSocketException : Exception
    {
        /// <summary>
        /// Creates a new websocket exception.
        /// </summary>
        public WebSocketException() : base()
        {

        }

        /// <summary>
        /// Creates a new websocket exception.
        /// </summary>
        /// <param name="message">The message.</param>
        public WebSocketException(string message) : base(message)
        {

        }

        /// <summary>
        /// Creates a new websocket exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public WebSocketException(string message, Exception innerException) : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates a new websocket exception.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected WebSocketException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}