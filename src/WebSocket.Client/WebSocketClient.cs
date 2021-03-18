namespace RoRamu.WebSocket.Client
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;

    /// <summary>
    /// A websocket client.
    /// </summary>
    public class WebSocketClient : WebSocketConnectionProxy
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
        /// If this is null, a default no-op controller will be used.
        /// </param>
        /// <param name="autoConnect">
        /// Connects to the server immediately if true, otherwise the <see cref="Connect" /> method
        /// must be called to connect this client to the server.
        /// </param>
        public WebSocketClient(
            WebSocketClientConnection connection,
            WebSocketController.FactoryDelegate controllerFactory = null,
            bool autoConnect = true)
            : base(connection, controllerFactory ?? DefaultControllerFactory)
        {
            this._socket = connection;
            this._socket.OnOpen = this.OnOpenInternal;
            this._socket.OnClose = this.OnCloseInternal;
            this._socket.OnError = this.OnErrorInternal;
            this._socket.OnMessage = this.OnMessageInternal;

            if (autoConnect)
            {
                this.Connect().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Connects to the websocket server (specified in the constructor).
        /// </summary>
        public async Task Connect()
        {
            this.Logger?.Log(LogLevel.Debug, $"Connecting", this);
            if (this.IsOpen())
            {
                this.Logger?.Log(LogLevel.Debug, $"Connection already open", this);
                return;
            }

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

        private static WebSocketController DefaultControllerFactory(IWebSocketConnection connection)
        {
            return new NoOpWebSocketController(connection);
        }
    }
}
