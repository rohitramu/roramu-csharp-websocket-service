namespace RoRamu.WebSocket
{
    using System;

    /// <summary>
    /// A message that indicates that a response is expected.
    /// </summary>
    public class Request : Message
    {
        internal Request(string id, string type, object body) : base(id, type, body)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        /// <summary>
        /// Creates a new request message.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="body">The body of the request.</param>
        /// <returns></returns>
        public Request(string type, object body) : base(Guid.NewGuid().ToString(), type, body)
        {
        }
    }
}
