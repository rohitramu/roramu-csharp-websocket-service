using System.Threading.Tasks;
using RoRamu.Utils.Messaging;

namespace RoRamu.WebSocket
{
    /// <summary>
    /// A WebSocket controller which does nothing.
    /// </summary>
    public class NoOpWebSocketController : WebSocketController
    {
        /// <summary>
        /// Creates a new <see cref="NoOpWebSocketController" />.
        /// </summary>
        /// <param name="webSocketConnection">The underlying <see cref="IWebSocketConnection" />.</param>
        public NoOpWebSocketController(IWebSocketConnection webSocketConnection) : base(webSocketConnection)
        {
        }

        /// <summary>
        /// Triggered when a message is received, but does nothing.
        /// </summary>
        /// <param name="message">The received message.</param>
        public override Task OnMessage(Message message)
        {
            //TODO: log

            return Task.CompletedTask;
        }
    }
}