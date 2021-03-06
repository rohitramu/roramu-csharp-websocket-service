namespace Test
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Messaging;
    using RoRamu.WebSocket;
    using RoRamu.WebSocket.Service;

    public class TestServiceController : WebSocketServiceController
    {
        private IMessageHandlerCollection MessageHandlerCollection { get; }

        public TestServiceController(string id, IWebSocketConnection connection) : base(id, connection)
        {
            this.Connection.RequestTimeout = TimeSpan.FromSeconds(5);

            this.MessageHandlerCollection = MessageHandlerCollectionBuilder
                .Create()
                .SetHandler("echo", async message => await this.Connection.SendMessage(message.CreateResponse(message.GetBody<object>())))
                .SetHandler("id", async message => await this.Connection.SendMessage(message.CreateResponse(id)))
                .SetHandler("Exception", (m) => throw new Exception($"You asked me to throw an exception:\n{m}"))
                .SetDefaultHandler(async message => await this.Connection.SendMessage(message.CreateResponse(new
                {
                    ThisIs = "A response",
                    RequestBody = message,
                })))
                .Build();
        }

        public override async Task OnMessage(Message message)
        {
            await this.MessageHandlerCollection.HandleMessage(message);
        }
    }
}