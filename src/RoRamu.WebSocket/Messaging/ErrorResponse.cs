namespace RoRamu.WebSocket
{
    using System;
    using Newtonsoft.Json.Linq;
    using RoRamu.Utils;

    /// <summary>
    /// Represents a message that is sent in response to another message.  Objects of this type
    /// would indicate that there was an error in processing the request message.
    /// </summary>
    public class ErrorResponse : Response
    {
        /// <summary>
        /// The message type.
        /// </summary>
        public new const string MessageType = WellKnownMessageTypes.Error;

        /// <summary>
        /// The exception representing the error.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// Creates a new error response message.
        /// </summary>
        /// <param name="error">The exception representing the error.</param>
        /// <param name="requestId">The ID of the request message.</param>
        /// <param name="includeStackTrace">
        /// Whether or not to include the stack trace in the response message.
        /// </param>
        public ErrorResponse(
            Exception error,
            string requestId,
            bool includeStackTrace = false)
            : base(
                  requestId,
                  new JRaw(error == null
                    ? throw new ArgumentNullException(nameof(error))
                    : error.ToJsonString(includeExceptionType: true, includeStackTrace: includeStackTrace)),
                  true)
        {
        }

        /// <summary>
        /// Creates a new object which represents an error response message.
        /// </summary>
        /// <param name="error">The exception representing the error.</param>
        /// <param name="request">The request message.</param>
        /// <param name="includeDebugInfo">
        /// Whether or not to include the stack trace in the response message.
        /// </param>
        /// <returns>The error response message.</returns>
        public static ErrorResponse Create(Message request, Exception error, bool includeDebugInfo = false)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            return new ErrorResponse(error, request.Id, includeDebugInfo);
        }
    }
}
