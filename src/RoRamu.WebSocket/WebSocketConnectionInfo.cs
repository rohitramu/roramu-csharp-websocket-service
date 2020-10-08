namespace RoRamu.WebSocket
{
    using System.Collections.Generic;

    /// <summary>
    /// Information about the connection, provided during the handshake by the client.
    /// </summary>
    public class WebSocketConnectionInfo
    {
        /// <summary>
        /// The remote endpoint in the <c>host:port</c> format.
        /// </summary>
        public string RemoteEndpoint { get; }

        /// <summary>
        /// The headers provided during the initial handshake request.
        /// </summary>
        public IReadOnlyCollection<KeyValuePair<string, string>> Headers { get; }

        /// <summary>
        /// The cookies provided during the initial handshake request.
        /// </summary>
        /// <value></value>
        public IReadOnlyCollection<KeyValuePair<string, string>> Cookies { get; }

        /// <summary>
        /// The path used in the URL when the connection was made.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Creates a new instance of <see cref="RoRamu.WebSocket.WebSocketConnectionInfo" />.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        /// <param name="path">The path as specified in the URL.</param>
        /// <param name="headers">The request headers.</param>
        /// <param name="cookies">The cookies in the request.</param>
        public WebSocketConnectionInfo(
            string remoteEndpoint = null,
            string path = null,
            ICollection<KeyValuePair<string, string>> headers = null,
            ICollection<KeyValuePair<string, string>> cookies = null)
        {
            this.RemoteEndpoint = remoteEndpoint;

            this.Path = path;

            this.Headers = headers == null
                ? new List<KeyValuePair<string, string>>()
                : new List<KeyValuePair<string, string>>(headers);

            this.Cookies = cookies == null
                ? new List<KeyValuePair<string, string>>()
                : new List<KeyValuePair<string, string>>(cookies);
        }
    }
}
