namespace RoRamu.WebSocket
{
    using System;

    public class Response : Message
    {
        public const string MessageType = WellKnownMessageTypes.Response;

        public Response(string requestId, object body, bool isError = false) : base(
            requestId,
            isError
                ? ErrorResponse.MessageType
                : Response.MessageType,
            body)
        {
        }

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
