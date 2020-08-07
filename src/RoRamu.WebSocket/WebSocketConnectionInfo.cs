namespace RoRamu.WebSocket
{
    using System;
    using System.Collections.Generic;

    public class WebSocketConnectionInfo
    {
        public string RemoteEndpoint { get; }

        public IReadOnlyCollection<KeyValuePair<string, string>> Headers { get; }

        public IReadOnlyCollection<KeyValuePair<string, string>> Cookies { get; }

        public string Path { get; }

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
