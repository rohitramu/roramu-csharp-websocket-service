namespace RoRamu.WebSocket
{
    using System;

    public class Request : Message
    {
        internal Request(string id, string type, object body) : base(id, type, body)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        public Request(string type, object body) : base(Guid.NewGuid().ToString(), type, body)
        {
        }
    }
}
