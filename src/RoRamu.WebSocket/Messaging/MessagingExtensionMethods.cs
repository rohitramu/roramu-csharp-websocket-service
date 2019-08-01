namespace RoRamu.WebSocket
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class MessagingExtensionMethods
    {
        public static readonly IEnumerable<string> ResponseMessageTypes = new HashSet<string>()
        {
            WellKnownMessageTypes.Response,
            WellKnownMessageTypes.Error,
        };

        public static bool IsSuccessResponse(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && message.Type == WellKnownMessageTypes.Response;
        }

        public static bool IsErrorResponse(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && message.Type == WellKnownMessageTypes.Error;
        }

        public static bool IsResponse(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && ResponseMessageTypes.Contains(message.Type);
        }

        public static bool IsRequest(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && !message.IsResponse();
        }

        public static bool TryParseResponse(this Message message, out Response response)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Check if this message represents a response
            if (message.IsResponse())
            {
                // Create the response and return true
                response = new Response(
                    message.Id,
                    message.Body,
                    message.Type == WellKnownMessageTypes.Error);
                return true;
            }
            else
            {
                // If it's not a response, return false
                response = null;
                return false;
            }
        }

        public static Response CreateResponse(this Message message, object body, bool isErrorResponse = false)
        {
            return new Response(message.Id, body, isErrorResponse);
        }
    }
}
