namespace Test
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.Utils.Messaging;
    using RoRamu.WebSocket;
    using RoRamu.WebSocket.Client;
    using RoRamu.WebSocket.Client.WebSocket4Net;

    public class TestClient : WebSocketClient
    {
        public TestClient(WebSocketConnectionInfo connectionInfo) : base(new WebSocket4NetConnection(connectionInfo), CreateWebSocketController)
        {
        }

        private static WebSocketController CreateWebSocketController(IWebSocketConnection connection)
        {
            return new TestClientController(connection);
        }

        public new async Task SendMessage(Message message)
        {
            this.Logger?.Log(LogLevel.Info, $"Sending message", message);
            await base.SendMessage(message);
        }

        public new async Task<RequestResult> SendRequest(Request request, TimeSpan? requestTimeout = null)
        {
            Logger?.Log(LogLevel.Info, $"Sending request '{request.Id}'", request);
            return await base.SendRequest(request, requestTimeout);
        }
    }
}