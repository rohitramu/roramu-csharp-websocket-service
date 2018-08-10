namespace RoRamu.WebSocket.Service
{
    using System;
    using Newtonsoft.Json.Linq;
    using RoRamu.Utils;

    public class ErrorResponse : Response
    {
        public new string MessageType { get; } = WellKnownMessageTypes.Error.ToString();

        public Exception Error { get; }

        public ErrorResponse(
            Exception error,
            Request request = null,
            bool includeStackTraceAndExceptionType = false)
            : base(request, new JRaw(error?.ToJsonString(includeStackTraceAndExceptionType)))
        {
            this.Error = error ?? throw new ArgumentNullException();
        }
    }
}
