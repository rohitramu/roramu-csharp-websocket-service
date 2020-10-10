namespace RoRamu.WebSocket
{
    using System;

    /// <summary>
    /// The result of sending a request and waiting for a response.
    /// </summary>
    public class RequestResult
    {
        /// <summary>
        /// The exception if either the response was never received or if an error was returned in
        /// the response message.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The request message.
        /// </summary>
        public Request Request { get; }

        /// <summary>
        /// The response message.
        /// </summary>
        public Response Response { get; }

        /// <summary>
        /// Whether or not the request completed successfully.
        /// </summary>
        public bool IsSuccessful => this.Exception == null && this.Response.Type != WellKnownMessageTypes.Error;

        private RequestResult(Request request, Response response, Exception exception)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Response = response;
            this.Exception = exception;
        }

        /// <summary>
        /// Creates a result object which indicates that the request received a response.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="response">The response object.</param>
        /// <returns>The result of the request.</returns>
        public static RequestResult Success(Request request, Response response)
        {
            return new RequestResult(
                request: request,
                response: response ?? throw new ArgumentNullException(nameof(response)),
                exception: null);
        }

        /// <summary>
        /// Creates a result object which indicates that an error occured before a response could
        /// be received.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="exception">The exception representing the error.</param>
        /// <returns>The result of the request.</returns>
        public static RequestResult Failure(Request request, Exception exception)
        {
            return new RequestResult(
                request: request,
                response: null,
                exception: exception ?? throw new ArgumentNullException(nameof(exception)));
        }
    }
}
