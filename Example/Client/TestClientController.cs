namespace Test
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Messaging;
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

        public override async Task OnMessage(Message message)
        {
            await this.Handlers.HandleMessage(message);
        }
    }
}