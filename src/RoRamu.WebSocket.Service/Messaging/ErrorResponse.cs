namespace RoRamu.WebSocket.Service
{
    using System;
    using Newtonsoft.Json.Linq;
    using RoRamu.Utils;

    public class ErrorResponse : Response
    {
        public new const string MessageType = WellKnownMessageTypes.Error;

        public Exception Error { get; }

        public ErrorResponse(
            Exception error,
            string requestId,
            bool includeDebugInfo = false)
            : base(
                  requestId,
                  new JRaw(error == null ? throw new ArgumentNullException(nameof(error)) : error.ToJsonString(includeDebugInfo)),
                  true)
        {
        }
    }
}
