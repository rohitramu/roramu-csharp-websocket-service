namespace RoRamu.WebSocket.Service
{
    using System;

    public class Request : Message
    {
        internal Request(string id, string messageType, object body) : base(id, messageType, body)
        {
        }

        public Request(string messageType, object body) : base(Guid.NewGuid().ToString(), messageType, body)
        {
        }
    }
}
