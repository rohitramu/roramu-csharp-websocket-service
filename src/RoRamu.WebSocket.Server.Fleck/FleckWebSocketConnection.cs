namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils;

    internal class FleckWebSocketConnection : WebSocketConnection
    {
        public const int MaxTextMessageLength = 1024 * 4;

        public override bool IsOpen => this._socket.IsAvailable;

        private readonly Fleck.IWebSocketConnection _socket;

        public FleckWebSocketConnection(Fleck.IWebSocketConnection socket)
        {
            this._socket = socket ?? throw new ArgumentNullException(nameof(socket));

            socket.OnMessage = message => this.OnMessage?.Invoke(message);
            socket.OnBinary = data => this.OnMessage?.Invoke(data.DecodeToString());
            socket.OnError = ex => this.OnError?.Invoke(ex);
            socket.OnClose = () => this.OnClose?.Invoke();
        }

        public override async Task Close()
        {
            await Task.Run(() => this._socket.Close());
        }

        public override async Task SendMessage(string message)
        {
            if (message.Length <= MaxTextMessageLength)
            {
                await this._socket.Send(message);
            }
            else
            {
                await this._socket.Send(message.Encode());
            }
        }

        public static WebSocketConnectionInfo CreateConnectionInfo(Fleck.IWebSocketConnection socket)
        {
            return new WebSocketConnectionInfo(
                $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}",
                socket.ConnectionInfo.Headers,
                socket.ConnectionInfo.Cookies);
        }
    }
}
