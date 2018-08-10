namespace RoRamu.WebSocket.Service
{
    using System;
    using RoRamu.Utils;

    public class ErrorResponse : Response
    {
        public new string MessageType { get; } = WellKnownMessageTypes.Error.ToString();

        public Exception Error { get; }

        public ErrorResponse(
            Exception error,
            bool includeStackTraceAndExceptionType = false,
            Request request = null)
            : base(request, error?.ToJsonString(includeStackTraceAndExceptionType))
        {
            this.Error = error ?? throw new ArgumentNullException();
        }
    }
}
