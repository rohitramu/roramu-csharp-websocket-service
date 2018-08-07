namespace RoRamu.WebSocket.Service.Messaging
{
    using System;

    public class Response : IMessage
    {
        public string MessageType { get; }

        public string Body { get; }

        public IRequest Request { get; }

        public Response(IRequest request, string body)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Body = body;
        }
    }
}
