namespace RoRamu.WebSocket.Service
{
    using System;

    public class WebSocketProxyValidationFailure : Exception
    {
        public WebSocketClientProxy InvalidProxy { get; }

        public WebSocketProxyValidationFailure(WebSocketClientProxy invalidProxy, string message) : base(message)
        {
            this.InvalidProxy = invalidProxy;
        }
    }
}
