namespace RoRamu.WebSocket
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods for <see cref="RoRamu.WebSocket.Message" /> objects.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// The list of message types which would indicate that a message is a response to a request.
        /// </summary>
        public static readonly IEnumerable<string> ResponseMessageTypes = new HashSet<string>()
        {
            WellKnownMessageTypes.Response,
            WellKnownMessageTypes.Error,
        };

        /// <summary>
        /// Whether or not this message represents a response indicating success.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// True if this message represents a response indicating success, otherwise false.
        /// </returns>
        public static bool IsSuccessResponse(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && message.Type == WellKnownMessageTypes.Response;
        }

        /// <summary>
        /// Whether or not this message represents a response indicating an error occured.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// True if this message represents a response indicating that an error occured, otherwise false.
        /// </returns>
        public static bool IsErrorResponse(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && message.Type == WellKnownMessageTypes.Error;
        }

        /// <summary>
        /// Whether or not this message represents a response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>True if this message represents a response, otherwise false.</returns>
        public static bool IsResponse(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && ResponseMessageTypes.Contains(message.Type);
        }

        /// <summary>
        /// Whether or not this message represents a request.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>True if this message represents a request, otherwise false.</returns>
        public static bool IsRequest(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && !message.IsResponse();
        }

        /// <summary>
        /// Attempts to parse a message as a response message.
        /// </summary>
        /// <param name="message">The message to try to parse.</param>
        /// <param name="response">
        /// The response message if parsing was successful, otherwise null.
        /// </param>
        /// <returns>True if parsing was successful, otherwise false.</returns>
        public static bool TryParseResponse(this Message message, out Response response)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Check if this message represents a response
            if (message.IsResponse())
            {
                // Create the response and return true
                response = new Response(
                    message.Id,
                    message.Body,
                    message.Type == WellKnownMessageTypes.Error);
                return true;
            }
            else
            {
                // If it's not a response, return false
                response = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a message which can be sent in response to this message.
        /// </summary>
        /// <param name="message">The request message.</param>
        /// <param name="body">The response body.</param>
        /// <param name="isErrorResponse">True if this is a response indicating that there was an
        /// error in processing the request message, otherwise false.</param>
        /// <returns>The response message.</returns>
        public static Response CreateResponse(this Message message, object body, bool isErrorResponse = false)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new Response(message.Id, body, isErrorResponse);
        }
    }
}
