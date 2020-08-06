namespace RoRamu.WebSocket
{
    using System;

    public class MessageHandlerCollectionBuilder
    {
        public delegate void HandlerDelegate(Message message);

        private MessageHandlerCollection MessageHandlerCollection { get; } = new MessageHandlerCollection();

        private MessageHandlerCollectionBuilder() { }

        public static MessageHandlerCollectionBuilder Create()
        {
            return new MessageHandlerCollectionBuilder();
        }

        public MessageHandlerCollectionBuilder SetFallbackHandler(HandlerDelegate messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            this.MessageHandlerCollection.FallbackMessageHandler = messageHandler;

            return this;
        }

        public MessageHandlerCollectionBuilder RemoveFallbackHandler()
        {
            this.MessageHandlerCollection.FallbackMessageHandler = null;

            return this;
        }

        public MessageHandlerCollectionBuilder SetHandler(string messageType, HandlerDelegate messageHandler)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            this.MessageHandlerCollection[messageType] = messageHandler;

            return this;
        }

        public MessageHandlerCollectionBuilder RemoveHandler(string messageType)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            this.MessageHandlerCollection.Remove(messageType);

            return this;
        }

        public IMessageHandlerCollection Build()
        {
            return this.MessageHandlerCollection;
        }
    }
}