namespace RoRamu.WebSocket
{
    /// <summary>
    /// The message types with special meaning in the protocol implemented by this library.
    /// </summary>
    public static class WellKnownMessageTypes
    {
        /// <summary>
        /// Indicates that the message was sent in response to another message which was processed
        /// successfully.
        /// </summary>
        public const string Response = nameof(Response);

        /// <summary>
        /// Indicates that the message was sent in response to another message which could not be
        /// successfully processed.
        /// </summary>
        public const string Error = nameof(Error);
    }
}
