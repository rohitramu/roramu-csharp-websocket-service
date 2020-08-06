namespace RoRamu.WebSocket.Service
{
    using System;

    public class WebSocketClientProxyWithMessageHandlers : WebSocketClientProxy
    {
        public IMessageHandlerCollection MessageHandlerCollection { get; }

        public WebSocketClientProxyWithMessageHandlers(
            string id,
            WebSocketActions proxyActions,
            IMessageHandlerCollection messageHandlerCollection) : base(id, proxyActions)
        {
            this.MessageHandlerCollection = messageHandlerCollection ?? throw new ArgumentNullException(nameof(messageHandlerCollection));
        }

        internal new void OnMessage(Message message)
        {
            this.MessageHandlerCollection.HandleMessage(message);
        }
    }
}