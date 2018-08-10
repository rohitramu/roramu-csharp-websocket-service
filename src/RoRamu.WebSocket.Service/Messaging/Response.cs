namespace RoRamu.WebSocket.Service
{
    using System;

    public class Response : Message
    {
        public Request Request { get; }

        public Response(Request request, object body) : base(request?.Id, WellKnownMessageTypes.Response.ToString(), body)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
        }
    }
}
