namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.Utils.Messaging;
    using RoRamu.WebSocket.Server;

    /// <summary>
    /// Defines the behavior of a websocket service.
    /// </summary>
    /// <typeparam name="TController">
    /// An implementation of a <see cref="RoRamu.WebSocket.Service.WebSocketServiceController" />.
    /// This defines how the service interacts with each connection.
    /// </typeparam>
    public abstract class WebSocketService<TController> where TController : WebSocketServiceController
    {
        /// <summary>
        /// The currently active websocket connection IDs mapped to their respective client proxies.
        /// </summary>
        public IReadOnlyDictionary<string, WebSocketClientProxy> Connections => this._connections;
        private readonly ConcurrentDictionary<string, WebSocketClientProxy> _connections = new ConcurrentDictionary<string, WebSocketClientProxy>();

        /// <summary>
        /// The underlying websocket server implementation.
        /// </summary>
        private readonly IWebSocketServer _server;

        /// <summary>
        /// The logger to be used.
        /// Defaults to <see cref="RoRamu.Utils.Logging.Logger.Default" />.
        /// If <c>null</c>, no logs will be emitted.
        /// </summary>
        public Logger Logger { get; set; } = Logger.Default;

        /// <summary>
        /// Constructs a new websocket service />.
        /// </summary>
        /// <param name="server">The server implementation to use when hosting the WebSocket service.</param>
        public WebSocketService(IWebSocketServer server)
        {
            this._server = server ?? throw new ArgumentNullException(nameof(server));
            this._server.OnOpen = OnOpen;
        }

        /// <summary>
        /// Starts the service.  Once the method completes, the service will be running.
        /// </summary>
        public async Task Start()
        {
            await this._server.Start();
        }

        /// <summary>
        /// Stops the service.  Once the method completes, the service will be stopped.
        /// </summary>
        public async Task Stop()
        {
            await this._server.Stop();
        }

        /// <summary>
        /// The logic used to handle a new incoming connection.
        /// </summary>
        /// <param name="socket">
        /// Represents the underlying websocket implementation's connection object.
        /// </param>
        /// <param name="connectionInfo">
        /// Information about the connection that was made by the client during the handshake.
        /// </param>
        private void OnOpen(WebSocketUnderlyingConnection socket, WebSocketConnectionInfo connectionInfo)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }
            if (connectionInfo == null)
            {
                throw new ArgumentNullException(nameof(connectionInfo));
            }

            Task.Run(async () =>
            {
                // Create the proxy for this connection
                WebSocketClientProxy proxy = await this.CreateProxy(socket, connectionInfo);

                // Set the proxy for this connection
                this._connections.AddOrUpdate(
                    key: proxy.Id,
                    addValue: proxy,
                    updateValueFactory: (id, oldProxy) =>
                    {
                        // Close the old connection and swallow any exceptions
                        this.Logger?.Log(LogLevel.Info, $"Closing duplicate connection to client '{id}'.");
                        oldProxy.Close().ContinueWith(closeTask =>
                        {
                            this.Logger?.Log(LogLevel.Warning, $"Failed to close duplicate connection to client '{id}'.", closeTask.Exception);
                        }, TaskContinuationOptions.OnlyOnFaulted);

                        return proxy;
                    });
            }).ContinueWith(createProxyTask =>
            {
                this.Logger?.Log(LogLevel.Error, "Failed to create client connection proxy.", createProxyTask.Exception);

                // Get the user-safe exceptions
                IEnumerable<UserSafeWebSocketException> userSafeExceptions = createProxyTask?.Exception?.InnerExceptions
                    ?.Where(e => e is UserSafeWebSocketException)
                    ?.Select(e => e as UserSafeWebSocketException);

                // Send the user-safe exceptions to the client
                if (userSafeExceptions != null && userSafeExceptions.Any())
                {
                    var errorMessage = new ErrorResponse(new AggregateException("Failed to connect", userSafeExceptions), requestId: null);
                }

                // Close the connection
                socket.Close().ContinueWith(closeTask =>
                {
                    this.Logger?.Log(LogLevel.Warning, $"Could not close the connection for which proxy creation failed.", closeTask.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RoRamu.WebSocket.Service.WebSocketClientProxy" /> to
        /// represent a new connection.
        /// </summary>
        /// <param name="socket">
        /// The <see cref="RoRamu.WebSocket.WebSocketUnderlyingConnection" /> which represents the underlying
        /// implementation of a websocket connection.
        /// </param>
        /// <param name="connectionInfo">
        /// Information about the connection that was made by the client during the handshake.  For
        /// example, this can be used to authenticate a client based on request headers, or assign
        /// different behaviors to the connection based on the path used in the URL.
        /// </param>
        /// <returns>
        /// The new <see cref="RoRamu.WebSocket.WebSocketUnderlyingConnection" /> representing the connection.
        /// </returns>
        private async Task<WebSocketClientProxy> CreateProxy(WebSocketUnderlyingConnection socket, WebSocketConnectionInfo connectionInfo)
        {
            // Generate the connection ID
            string connectionId;
            try
            {
                connectionId = this.GenerateConnectionId(connectionInfo) ?? throw new ArgumentNullException(nameof(connectionId));
            }
            catch
            {
                this.Logger?.Log(LogLevel.Error, $"Failed to generate connectionId.", connectionInfo);
                throw;
            }

            // Create the proxy
            WebSocketClientProxy proxy = new WebSocketClientProxy(
                connectionId,
                socket,
                connection => this.CreateController(connectionId, connectionInfo, connection));

            // Attach the event handlers
            socket.OnClose = proxy.OnCloseInternal;
            socket.OnError = proxy.OnErrorInternal;
            socket.OnMessage = proxy.OnMessageInternal;

            // Run the "OnOpen()" method
            await proxy.OnOpenInternal();

            return proxy;
        }

        /// <summary>
        /// Generates a unique string to represent a connection (duplicate connections will be terminated automatically).
        /// </summary>
        /// <param name="connectionInfo">Information about the connection.</param>
        /// <returns>A connection ID.</returns>
        protected virtual string GenerateConnectionId(WebSocketConnectionInfo connectionInfo)
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new instance of a controller, which defines the behavior of the service for a connection.
        /// </summary>
        /// <param name="connectionId">The connection's ID.</param>
        /// <param name="connectionInfo">Information about the connection.</param>
        /// <param name="connection">
        /// Actions available for interacting with this websocket connection.
        /// </param>
        /// <returns>A controller.</returns>
        protected abstract TController CreateController(
            string connectionId,
            WebSocketConnectionInfo connectionInfo,
            IWebSocketConnection connection);
    }
}
