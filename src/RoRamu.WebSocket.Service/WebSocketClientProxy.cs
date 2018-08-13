namespace RoRamu.WebSocket.Service
{
    using System;

    public class WebSocketClientProxy : WebSocketConnectionProxy
    {
        public string Id { get; }

        public WebSocketClientProxy(string id, WebSocketActions proxyActions) : base(proxyActions)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        internal new void OnOpenInternal() => base.OnOpenInternal();

        internal new void OnCloseInternal() => base.OnCloseInternal();

        internal new void OnErrorInternal(Exception error) => base.OnErrorInternal(error);

        internal new void OnMessageInternal(string stringMessage) => base.OnMessageInternal(stringMessage);
    }
}
