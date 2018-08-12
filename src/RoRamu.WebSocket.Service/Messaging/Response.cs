namespace RoRamu.WebSocket.Service
{
    public class Response : Message
    {
        public static string MessageType { get; } = WellKnownMessageTypes.Response;

        public Response(string requestId, object body, bool isError = false) : base(requestId, isError ? ErrorResponse.MessageType : Response.MessageType, body)
        {
        }
    }
}
