namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Runtime.Serialization;

    public class WebSocketProxyValidationException : WebSocketException
    {
        public WebSocketConnectionProxy InvalidProxy { get; }

        public WebSocketProxyValidationException(WebSocketConnectionProxy invalidProxy, string message) : base(message)
        {
            this.InvalidProxy = invalidProxy;
        }

        public WebSocketProxyValidationException(WebSocketConnectionProxy invalidProxy, string message, Exception innerException) : base(message, innerException)
        {
            this.InvalidProxy = invalidProxy;
        }

        protected WebSocketProxyValidationException(WebSocketConnectionProxy invalidProxy, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.InvalidProxy = invalidProxy;
        }
    }
}
