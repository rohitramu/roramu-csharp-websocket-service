namespace RoRamu.WebSocket
{
    using System;

    public class UnknownMessageTypeException : Exception
    {
        public string MessageType { get; }

        internal UnknownMessageTypeException(string messageType) : base($"Unknown message type: {messageType}")
        {
            this.MessageType = messageType;
        }
    }
}