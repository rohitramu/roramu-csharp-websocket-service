namespace RoRamu.WebSocket
{
    using System.Collections.Generic;

    public interface IMessageHandlerCollection
    {
        /// <summary>
        /// The list of currently mapped message types.
        /// </summary>
        /// <value>The list of currently mapped message types.</value>
        IEnumerable<string> MappedMessageTypes { get; }

        /// <summary>
        /// Handles a message with the mapped handler for the message type.
        /// If no handler is found, the default message handler is used, which throws an
        /// <see cref="RoRamu.WebSocket.UnknownMessageTypeException"/>.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        void HandleMessage(Message message);
    }
}