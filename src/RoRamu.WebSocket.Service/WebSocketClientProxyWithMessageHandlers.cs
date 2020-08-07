namespace RoRamu.WebSocket.Service
{
    using System;

    public abstract class WebSocketClientProxyWithMessageHandlers : WebSocketClientProxy
    {
        private IMessageHandlerCollection MessageHandlers { get; }

        public WebSocketClientProxyWithMessageHandlers(
            string id,
            WebSocketActions proxyActions) : base(id, proxyActions)
        {
            this.MessageHandlers = this.SetupMessageHandlers();

            if (this.MessageHandlers == null)
            {
                throw new ArgumentNullException($"{nameof(SetupMessageHandlers)} must not return null.");
            }
        }

        public abstract IMessageHandlerCollection SetupMessageHandlers();

        public sealed override void OnMessage(Message message)
        {
            this.MessageHandlers.HandleMessage(message);
        }
    }
}