namespace RoRamu.WebSocket
{
    using System;

    public static class WebSocketConnectionExtensions
    {
        public static WebSocketActions ToWebSocketActions(this WebSocketConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return new WebSocketActions(
                isOpenFunc: () => connection.IsOpen,
                sendMessageFunc: (message) => connection.SendMessage(message.ToJsonString()),
                closeFunc: () => connection.Close());
        }
    }
}
