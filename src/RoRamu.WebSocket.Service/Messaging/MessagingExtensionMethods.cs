namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Collections.Generic;

    internal static class MessagingExtensionMethods
    {
        private static readonly ISet<string> ResponseMessageTypes = new HashSet<string>()
        {
            WellKnownMessageTypes.Response,
            WellKnownMessageTypes.Error,
        };

        internal static bool IsResponse(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.Id != null && ResponseMessageTypes.Contains(message.Type);
        }

        internal static bool TryParseResponse(this Message message, out Response response)
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
    }
}
