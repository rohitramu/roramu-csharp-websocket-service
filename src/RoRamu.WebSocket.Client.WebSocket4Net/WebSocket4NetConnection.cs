using WebSocket4NetImpl = WebSocket4Net;

namespace RoRamu.WebSocket.Client.WebSocket4Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RoRamu.Utils;

    public class WebSocket4NetConnection : WebSocketClientConnection
    {
        public const int MaxTextMessageLength = 1024 * 4;

        public override bool IsOpen => this._socket.State == WebSocket4NetImpl.WebSocketState.Open;

        private readonly WebSocket4NetImpl.WebSocket _socket;

        private static readonly ISet<string> WebSocketSchemes = new HashSet<string>() { "ws", "wss" };
        private static readonly string FormattedWebSocketSchemesList = string.Join(", ", WebSocketSchemes.Select(scheme => $"'{scheme}'"));

        public WebSocket4NetConnection(WebSocketConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException(nameof(connectionInfo));
            }
            if (!Uri.TryCreate(connectionInfo.RemoteEndpoint, UriKind.Absolute, out Uri endpoint))
            {
                throw new ArgumentException("The provided websocket endpoint is not a well-formed URI string", nameof(connectionInfo));
            }
            if (!WebSocketSchemes.Contains(endpoint.Scheme))
            {
                throw new ArgumentException($"The endpoint scheme must be one of the following: {FormattedWebSocketSchemesList}", nameof(connectionInfo));
            }

            this._socket = new WebSocket4NetImpl.WebSocket(
                uri: connectionInfo.RemoteEndpoint,
                customHeaderItems: new List<KeyValuePair<string, string>>(connectionInfo.Headers),
                cookies: new List<KeyValuePair<string, string>>(connectionInfo.Cookies));

            this._socket.MessageReceived += (sender, messageArgs) => this.OnMessage?.Invoke(messageArgs.Message);
            this._socket.DataReceived += (sender, dataArgs) => this.OnMessage?.Invoke(dataArgs.Data.DecodeToString());
            this._socket.Error += (sender, errorArgs) => this.OnError?.Invoke(errorArgs.Exception);
            this._socket.Closed += (sender, args) => this.OnClose?.Invoke();
            this._socket.Opened += (sender, args) => this.OnOpen?.Invoke();
        }

        public override async Task Connect()
        {
            await this._socket.OpenAsync();
        }

        public override async Task Close()
        {
            await Task.Run(() =>
            {
                try
                {
                    this._socket.Close();
                }
                finally
                {
                    this._socket.Dispose();
                }
            });
        }

        public override Task SendMessage(string message)
        {
            return Task.Run(() =>
            {
                if (message.Length <= MaxTextMessageLength)
                {
                    this._socket.Send(message);
                }
                else
                {
                    byte[] data = message.Encode();
                    this._socket.Send(data, 0, data.Length);
                }
            });
        }
    }
}
