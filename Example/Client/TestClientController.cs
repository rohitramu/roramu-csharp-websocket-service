namespace Test
{
    using RoRamu.WebSocket;

    public class TestClientController : WebSocketController
    {
        private IMessageHandlerCollection Handlers { get; }

        public TestClientController(IWebSocketConnection connection) : base(connection)
        {
            this.Handlers = MessageHandlerCollectionBuilder
                .Create()
                .Build();
        }

        public override void OnMessage(Message message)
        {
            this.Handlers.HandleMessage(message);
        }
    }
}