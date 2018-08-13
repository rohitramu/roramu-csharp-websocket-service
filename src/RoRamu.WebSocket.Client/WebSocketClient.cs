namespace RoRamu.WebSocket.Client
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;

    public abstract class WebSocketClient : WebSocketConnectionProxy
    {
        private readonly WebSocketClientConnection _socket;

        public WebSocketClient(WebSocketClientConnection connection) : base(connection.ToWebSocketActions())
        {
            this._socket = connection;
            this._socket.OnOpen = this.OnOpenInternal;
            this._socket.OnClose = this.OnCloseInternal;
            this._socket.OnError = this.OnErrorInternal;
            this._socket.OnMessage = this.OnMessageInternal;
        }

        public async Task Connect()
        {
            this.Logger?.Log(LogLevel.Debug, $"Connecting", this);
            try
            {
                await this._socket.Connect();
                this.Logger?.Log(LogLevel.Debug, $"Connected", this);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to connect", ex);
            }
        }
    }
}
