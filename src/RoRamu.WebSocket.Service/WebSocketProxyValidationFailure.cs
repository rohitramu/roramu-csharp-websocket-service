namespace RoRamu.WebSocket.Service
{
    using System;

    public class WebSocketProxyValidationFailure : Exception
    {
        public WebSocketConnectionProxy InvalidProxy { get; }

        public WebSocketProxyValidationFailure(WebSocketConnectionProxy invalidProxy, string message) : base(message)
        {
            this.InvalidProxy = invalidProxy;
        }
    }
}
