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

        public async Task Stop()
        {
            await this._server.Stop();
        }

        private void OnOpen(WebSocketConnection socket, WebSocketConnectionInfo connectionInfo)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }
            if (connectionInfo == null)
            {
                throw new ArgumentNullException(nameof(connectionInfo));
            }

            Task.Run(() =>
            {
                // Create and set the proxy for this connection
                TClientProxy proxy = this.CreateProxy(socket, connectionInfo);
                this._connections.AddOrUpdate(
                    key: proxy.Id,
                    addValue: proxy,
                    updateValueFactory: (id, oldProxy) =>
                    {
                        // Close the old connection and swallow any exceptions
                        oldProxy.Close().ContinueWith(closeTask =>
                        {
                            this.Logger?.Log(LogLevel.Warning, $"Failed to close duplicate connection to client '{id}'", closeTask.Exception);
                        }, TaskContinuationOptions.OnlyOnFaulted);

                        return proxy;
                    });
            }).ContinueWith(createProxyTask =>
            {
                this.Logger?.Log(LogLevel.Error, "Failed to create client connection proxy", createProxyTask.Exception);
                socket.Close().ContinueWith(closeTask =>
                {
                    this.Logger?.Log(LogLevel.Warning, $"Could not close the connection for which proxy creation failed", closeTask.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private TClientProxy CreateProxy(WebSocketConnection socket, WebSocketConnectionInfo connectionInfo)
        {
            // Create the proxy actions
            WebSocketActions proxyActions = new WebSocketActions(
                isOpenFunc:         () => socket.IsOpen,
                sendMessageFunc:    (message) => socket.SendMessage(message.ToJsonString()),
                closeFunc:          () => socket.Close());

            // Create the proxy
            TClientProxy proxy = this.CreateProxy(connectionInfo, proxyActions);

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

        private void ValidateProxy(TClientProxy proxy, WebSocketConnection socket)
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
            WebSocketConnectionInfo connectionInfo,
            WebSocketActions proxyActions);
    }
}
