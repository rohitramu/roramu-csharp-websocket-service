namespace RoRamu.WebSocket.Service
{
    using System;

    public class WebSocketProxyValidationFailure : Exception
    {
        public IWebSocketProxy InvalidProxy { get; }

        public WebSocketProxyValidationFailure(IWebSocketProxy invalidProxy, string message) : base(message)
        {
            this.InvalidProxy = invalidProxy;
        }
    }
}
