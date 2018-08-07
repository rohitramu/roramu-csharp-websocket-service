namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.WebSocket.Server;

    public abstract class WebSocketService<TWebSocketProxy> : IWebSocketService<TWebSocketProxy> where TWebSocketProxy : class, IWebSocketProxy
    {
        /// <summary>
        /// The currently active websocket connection IDs mapped to their respective client proxies.
        /// </summary>
        public IReadOnlyDictionary<string, TWebSocketProxy> Connections => this._connections;
        private readonly ConcurrentDictionary<string, TWebSocketProxy> _connections = new ConcurrentDictionary<string, TWebSocketProxy>();

        private readonly IWebSocketServer _server;

        public Logger Logger { get; set; } = Logger.DefaultLogger;

        public WebSocketService(IWebSocketServer server)
        {
            this._server = server ?? throw new ArgumentNullException(nameof(server));
            this._server.OnOpen = OnOpen;
        }

        public async Task Start()
        {
            await this._server.Start();
        }

        public void Stop()
        {
            this._server.Stop();
        }

        private void OnOpen(IWebSocket socket)
        {
            // Set the proxy for this connection
            TWebSocketProxy proxy = this.CreateProxy(socket);
            this._connections.AddOrUpdate(
                key: proxy.Id,
                addValue: proxy,
                updateValueFactory: (id, oldProxy) =>
                {
                    // Close the old connection and swallow any exceptions
                    try { oldProxy.Close(); } catch { }

                    return proxy;
                });

            // Validate the proxy
            string errorMessage = null;
            if (proxy == null)
            {
                errorMessage = $"The {nameof(CreateProxy)}() method returned null in the '{this.GetType().FullName}' implementation";
            }
            else if (proxy.Id == null)
            {
                errorMessage = $"The {nameof(CreateProxy)}() method returned a client proxy with a null {nameof(IWebSocketProxy.Id)} in the '{this.GetType().FullName}' implementation";
            }

            // Throw an exception if the proxy is invalid
            if (errorMessage != null)
            {
                try
                {
                    socket.Close();
                }
                finally
                {
                    throw new WebSocketProxyValidationFailure(proxy, errorMessage);
                }
            }
        }

        private TWebSocketProxy CreateProxy(IWebSocket socket)
        {
            // Create the proxy
            TWebSocketProxy proxy = this.CreateProxy(
                socket.ClientIpAddress,
                socket.Headers,
                socket.Cookies,
                () => socket.IsOpen,
                socket.SendMessage,
                socket.Close);

            // Attach the event handlers
            socket.OnClose = proxy.OnClose;
            socket.OnError = proxy.OnError;
            socket.OnMessage = proxy.OnMessage;

            // Run the "OnOpen()" method
            proxy.OnOpen();

            return proxy;
        }

        protected abstract TWebSocketProxy CreateProxy(
            string clientIpAddress,
            IReadOnlyDictionary<string, string> headers,
            IReadOnlyDictionary<string, string> cookies,
            IsOpen isOpenFunc,
            SendMessage sendMessageFunc,
            Close closeFunc);

        public async Task Broadcast(string message)
        {
            IList<Task> tasks = new List<Task>();
            foreach (TWebSocketProxy connection in this.Connections.Values)
            {
                tasks.Add(connection.SendMessage(message));
            }

            await Task.WhenAll(tasks);
        }
    }
}
