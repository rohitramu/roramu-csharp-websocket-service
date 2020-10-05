namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Runtime.Serialization;

    public class WebSocketException : Exception
    {
        public WebSocketException() : base()
        {

        }

        public WebSocketException(string message) : base(message)
        {

        }

        public WebSocketException(string message, Exception innerException) : base(message, innerException)
        {

        }

        protected WebSocketException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}