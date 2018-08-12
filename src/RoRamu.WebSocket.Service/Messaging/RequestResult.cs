namespace RoRamu.WebSocket.Service
{
    using System;

    public class RequestResult
    {
        public Exception Exception { get; }
        
        public Request Request { get; }

        public Response Response { get; }

        public bool IsSuccessful => this.Exception == null && this.Response.Type != WellKnownMessageTypes.Error;

        private RequestResult(Request request, Response response, Exception exception)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Response = response;
            this.Exception = exception;
        }

        public static RequestResult Success(Request request, Response response)
        {
            return new RequestResult(
                request: request,
                response: response ?? throw new ArgumentNullException(nameof(response)),
                exception: null);
        }

        public static RequestResult Failure(Request request, Exception exception)
        {
            return new RequestResult(
                request: request,
                response: null,
                exception: exception ?? throw new ArgumentNullException(nameof(exception)));
        }
    }
}
