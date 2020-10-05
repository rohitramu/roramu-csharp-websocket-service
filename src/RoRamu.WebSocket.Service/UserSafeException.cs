namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Runtime.Serialization;

    public class UserSafeWebSocketException : WebSocketException
    {
        public UserSafeWebSocketException() : base()
        {

        }

        public UserSafeWebSocketException(string message) : base(message)
        {

        }

        public UserSafeWebSocketException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected UserSafeWebSocketException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}