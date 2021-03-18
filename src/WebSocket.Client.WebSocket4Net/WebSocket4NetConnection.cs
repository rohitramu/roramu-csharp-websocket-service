using WebSocket4NetImpl = WebSocket4Net;

namespace RoRamu.WebSocket.Client.WebSocket4Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RoRamu.Utils;

    /// <summary>
    /// An adapter for implementing a websocket client using the WebSocket4Net library:
    /// <see href="https://github.com/kerryjiang/WebSocket4Net" />.
    /// </summary>
    public class WebSocket4NetConnection : WebSocketClientConnection
    {
        /// <summary>
        /// The maximum size for a message sent as text (i.e. not as a byte stream).
        /// </summary>
        public const int MaxTextMessageLength = 1024 * 4;

        /// <summary>
        /// Whether or not the connection to the server is open.
        /// </summary>
        /// <returns>True if the connection is open, otherwise false.</returns>
        public override bool IsOpen() => this._socket.State == WebSocket4NetImpl.WebSocketState.Open;

        private readonly WebSocket4NetImpl.WebSocket _socket;

        private static readonly ISet<string> WebSocketSchemes = new HashSet<string>() { "ws", "wss" };
        private static readonly string FormattedWebSocketSchemesList = string.Join(", ", WebSocketSchemes.Select(scheme => $"'{scheme}'"));

        /// <summary>
        /// Creates a new <see cref="RoRamu.WebSocket.Client.WebSocket4Net.WebSocket4NetConnection" /> object.
        /// </summary>
        /// <param name="connectionInfo">Information about how to connect to the websocket server.</param>
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

            this._socket.MessageReceived += async (sender, messageArgs) => await this.OnMessage?.Invoke(messageArgs.Message);
            this._socket.DataReceived += async (sender, dataArgs) => await this.OnMessage?.Invoke(dataArgs.Data.DecodeToString());
            this._socket.Error += async (sender, errorArgs) => await this.OnError?.Invoke(errorArgs.Exception);
            this._socket.Closed += async (sender, args) => await this.OnClose?.Invoke();
            this._socket.Opened += async (sender, args) => await this.OnOpen?.Invoke();
        }

        /// <summary>
        /// Opens a connection to the websocket server.
        /// </summary>
        public override async Task Connect()
        {
            await this._socket.OpenAsync();
        }

        /// <summary>
        /// Closes the connection to the server if it is open, otherwise does nothing.
        /// </summary>
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

        /// <summary>
        /// Sends a text message to the websocket server.
        /// </summary>
        /// <param name="message">The message to send.</param>
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
