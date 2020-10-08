namespace RoRamu.WebSocket
{
    using System;

    /// <summary>
    /// Represents a message that is sent in response to another message.  Objects of this type
    /// would indicate that the request message was processed without error.
    /// </summary>
    public class Response : Message
    {
        /// <summary>
        /// The message type.
        /// </summary>
        public const string MessageType = WellKnownMessageTypes.Response;

        /// <summary>
        /// Creates a new response message.
        /// </summary>
        /// <param name="requestId">
        /// The ID of the request message that this message is responding to.
        /// </param>
        /// <param name="body">The body of the response.</param>
        /// <param name="isError">
        /// Whether or not this message is indicating that an error occured while processing the
        /// request message.
        /// </param>
        public Response(string requestId, object body, bool isError = false) : base(
            requestId,
            isError
                ? ErrorResponse.MessageType
                : Response.MessageType,
            body)
        {
        }

        /// <summary>
        /// Creates a new response message.
        /// </summary>
        /// <param name="request">The request message that this message is responding to.</param>
        /// <param name="responseBody">The body to be sent in this response message.</param>
        /// <returns></returns>
        public static Response Create(Message request, object responseBody)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new Response(request.Id, responseBody);
        }
    }
}
