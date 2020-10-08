namespace RoRamu.WebSocket.Client
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;

    /// <summary>
    /// A websocket client.
    /// </summary>
    public abstract class WebSocketClient : WebSocketConnectionProxy
    {
        /// <summary>
        /// The underlying websocket client implementation's connection object.
        /// </summary>
        private readonly WebSocketClientConnection _socket;

        /// <summary>
        /// Creates a new websocket client.
        /// </summary>
        /// <param name="connection">The connection to be made to the websocket server.</param>
        /// <param name="controllerFactory">
        /// A factory method that creates a controller which implements the client's behavior.
        /// </param>
        /// <returns></returns>
        public WebSocketClient(
            WebSocketClientConnection connection,
            WebSocketController.FactoryDelegate controllerFactory)
            : base(connection, controllerFactory)
        {
            this._socket = connection;
            this._socket.OnOpen = this.OnOpenInternal;
            this._socket.OnClose = this.OnCloseInternal;
            this._socket.OnError = this.OnErrorInternal;
            this._socket.OnMessage = this.OnMessageInternal;
        }

        /// <summary>
        /// Connects to the websocket server (specified in the constructor).
        /// </summary>
        public async Task Connect()
        {
            this.Logger?.Log(LogLevel.Debug, $"Connecting", this);
            try
            {
                await this._socket.Connect();
                this.Logger?.Log(LogLevel.Info, $"Connected", this);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to connect", ex);
            }
        }
    }
}
