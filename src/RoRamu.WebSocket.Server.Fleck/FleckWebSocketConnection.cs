using FleckImpl = Fleck;

namespace RoRamu.WebSocket.Server.Fleck
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils;

    internal class FleckWebSocketConnection : WebSocketUnderlyingConnection
    {
        public const int MaxTextMessageLength = 1024 * 4;

        public override bool IsOpen() => this._socket.IsAvailable;

        private readonly FleckImpl.IWebSocketConnection _socket;

        public FleckWebSocketConnection(FleckImpl.IWebSocketConnection socket)
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

        public static WebSocketConnectionInfo CreateConnectionInfo(FleckImpl.IWebSocketConnection socket)
        {
            return new WebSocketConnectionInfo(
                $"{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}",
                socket.ConnectionInfo.Path,
                socket.ConnectionInfo.Headers,
                socket.ConnectionInfo.Cookies);
        }
    }
}
