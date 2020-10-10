namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception which contains a message and typename that is safe to share with websocket clients.
    /// </summary>
    public class UserSafeWebSocketException : WebSocketException
    {
        /// <summary>
        /// Creates a new user-safe websocket exception.
        /// </summary>
        public UserSafeWebSocketException() : base()
        {

        }

        /// <summary>
        /// Creates a new user-safe websocket exception.
        /// </summary>
        /// <param name="message">The message.</param>
        public UserSafeWebSocketException(string message) : base(message)
        {

        }

        /// <summary>
        /// Creates a new user-safe websocket exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UserSafeWebSocketException(string message, Exception innerException) : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates a new user-safe websocket exception.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected UserSafeWebSocketException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}