namespace Test
{
    using System;
    using RoRamu.WebSocket;
    using RoRamu.WebSocket.Service;
    using RoRamu.WebSocket.Server.Fleck;

    public class TestService : WebSocketService<TestServiceController>
    {
        public TestService() : base(new FleckWebSocketServerAdapter())
        {
        }

        protected override string GenerateConnectionId(WebSocketConnectionInfo connectionInfo)
        {
            return Guid.NewGuid().ToString();
        }

        protected override TestServiceController CreateController(
            string connectionId,
            WebSocketConnectionInfo connectionInfo,
            IWebSocketConnection connection)
        {
            return new TestServiceController(connectionId, connection);
        }
    }
}