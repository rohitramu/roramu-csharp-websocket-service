namespace RoRamu.WebSocket
{
    using System;
    using System.Collections.Generic;

    internal class MessageHandlerCollection : Dictionary<string, MessageHandlerCollectionBuilder.HandlerDelegate>, IMessageHandlerCollection
    {
        public IEnumerable<string> MappedMessageTypes => this.Keys;

        /// <summary>
        /// The default message handler to use if one hasn't been mapped for the provided message type.
        /// </summary>
        public static MessageHandlerCollectionBuilder.HandlerDelegate DefaultFallbackMessageHandler = (message) =>
        {
            throw new UnknownMessageTypeException(message.Type);
        };

        internal MessageHandlerCollectionBuilder.HandlerDelegate FallbackMessageHandler { get; set; }

        /// <summary>
        /// Handles a message with the mapped handler if one is available, otherwise uses the
        /// default handler.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        public void HandleMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!this.TryGetValue(message.Type, out MessageHandlerCollectionBuilder.HandlerDelegate handler))
            {
                handler = DefaultFallbackMessageHandler;
            }

            handler(message);
        }
    }
}