namespace RoRamu.WebSocket
{
    using System;

    /// <summary>
    /// The exception that is thrown by the default message handler when no handler is registered
    /// for an incoming message's "type" property.
    /// </summary>
    public class UnknownMessageTypeException : Exception
    {
        /// <summary>
        /// The message type for which there was no message handler.
        /// </summary>
        public string MessageType { get; }

        internal UnknownMessageTypeException(string messageType) : base($"Unknown message type: {messageType}")
        {
            this.MessageType = messageType;
        }
    }
}