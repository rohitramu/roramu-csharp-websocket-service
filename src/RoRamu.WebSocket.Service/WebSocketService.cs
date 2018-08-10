namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.WebSocket.Server;

    public abstract class WebSocketService<TClientProxy> where TClientProxy : WebSocketClientProxy
    {
        /// <summary>
        /// The currently active websocket connection IDs mapped to their respective client proxies.
        /// </summary>
        public IReadOnlyDictionary<string, TClientProxy> Connections => this._connections;
        private readonly ConcurrentDictionary<string, TClientProxy> _connections = new ConcurrentDictionary<string, TClientProxy>();

        private readonly IWebSocketServer _server;

        public Logger Logger { get; set; } = Logger.Default;

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

        private void OnOpen(WebSocket socket)
        {
            Task.Run(() =>
            {
                // Create the proxy for this connection
                return this.CreateProxy(socket);
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    this.Logger?.Log(LogLevel.Error, "Failed to create client connection proxy", task.Exception);
                }
                else
                {
                    // Set the proxy for this connection
                    TClientProxy proxy = task.Result;
                    this._connections.AddOrUpdate(
                    key: proxy.Id,
                    addValue: proxy,
                    updateValueFactory: (id, oldProxy) =>
                    {
                        // Close the old connection and swallow any exceptions
                        Task.Run(async () =>
                        {
                            try
                            {
                                await oldProxy.Close();
                            }
                            catch (Exception ex)
                            {
                                this.Logger?.Log(LogLevel.Warning, $"Failed to close duplicate connection to client with ID: {id}", ex);
                            }
                        }).ConfigureAwait(false);

                        return proxy;
                    });
                }
            });
        }

        private TClientProxy CreateProxy(WebSocket socket)
        {
            // Create the proxy actions
            WebSocketProxyActions proxyActions = new WebSocketProxyActions(
                isOpenFunc: () => socket.IsOpen(),
                sendMessageFunc: (message) => socket.SendMessage(message.ToJsonString()),
                closeFunc: () => socket.Close());

            // Create the proxy
            TClientProxy proxy = this.CreateProxy(
                socket.ClientIpAddress,
                socket.Headers,
                socket.Cookies,
                proxyActions);

            // Validate the proxy
            ValidateProxy(proxy, socket);

            // Attach the event handlers
            socket.OnClose = proxy.OnCloseInternal;
            socket.OnError = proxy.OnErrorInternal;
            socket.OnMessage = proxy.OnMessageInternal;

            // Run the "OnOpen()" method
            proxy.OnOpenInternal();

            return proxy;
        }

        private void ValidateProxy(TClientProxy proxy, WebSocket socket)
        {
            string errorMessage = null;
            if (proxy == null)
            {
                errorMessage = $"The {nameof(CreateProxy)}() method returned null in the '{this.GetType().FullName}' implementation";
            }
            else if (proxy.Id == null)
            {
                errorMessage = $"The {nameof(CreateProxy)}() method returned a client proxy with a null {nameof(WebSocketClientProxy.Id)} in the '{this.GetType().FullName}' implementation";
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

        protected abstract TClientProxy CreateProxy(
            string clientIpAddress,
            IReadOnlyDictionary<string, string> headers,
            IReadOnlyDictionary<string, string> cookies,
            WebSocketProxyActions proxyActions);
    }
}
