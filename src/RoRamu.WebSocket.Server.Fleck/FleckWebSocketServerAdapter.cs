using FleckImpl = Fleck;

namespace RoRamu.WebSocket.Server.Fleck
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;

    /// <summary>
    /// An adapter for a websocket server using the Fleck library:
    /// https://github.com/statianzo/Fleck.
    /// </summary>
    public sealed class FleckWebSocketServerAdapter : IWebSocketServer
    {
        /// <summary>
        /// A callback for when a connection is opened by a client.
        /// </summary>
        /// <value>The method to be called when a connection is opened by a client.</value>
        public Action<WebSocketUnderlyingConnection, WebSocketConnectionInfo> OnOpen { get; set; }

        /// <summary>
        /// The default port to listen on when listening for secure (i.e. <c>wss://</c>) connections.
        /// </summary>
        public const int DefaultPortSecured = 443;
        /// <summary>
        /// The default port to listen on when listening for unsecure (i.e. <c>ws://</c>) connections.
        /// </summary>
        public const int DefaultPortUnsecured = 80;

        /// <summary>
        /// The scheme used when listening for secure connections.
        /// </summary>
        public const string WebSocketSchemeSecured = "wss";
        /// <summary>
        /// The scheme used when listening for unsecure connections.
        /// </summary>
        public const string WebSocketSchemeUnsecured = "ws";

        /// <summary>
        /// The logger to be used.
        /// Defaults to <see cref="RoRamu.Utils.Logging.Logger.Default" />.
        /// If <c>null</c>, no logs will be emitted.
        /// </summary>
        public Logger Logger { get; set; } = Logger.Default;

        /// <summary>
        /// The port to listen on for new websocket connections.
        /// If not provided, a default will be chosen based on the
        /// <see cref="RoRamu.WebSocket.Server.Fleck.FleckWebSocketServerAdapter.IsSecure" />
        /// property, which in turn depends on whether an SSL certificate was provided.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The SSL certificate to be used when listening for secure connections.
        /// </summary>
        public X509Certificate2 Certificate { get; }

        /// <summary>
        /// Whether or not the server is listening for secure connections (i.e. whether it is using
        /// SSL or not).
        /// </summary>
        public bool IsSecure { get; }

        /// <summary>
        /// Whether or not the server is running.
        /// </summary>
        public bool IsRunning => this._server != null;

        private string WebSocketScheme { get; }

        private string Location => $"{this.WebSocketScheme}://0.0.0.0:{this.Port}";

        private FleckImpl.WebSocketServer _server;

        private readonly object _lock = new object();

        /// <summary>
        /// Creates a new instance of a Fleck server which is wrapped in this adapter object.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <param name="certificate">The SSL certificate to use when establishing connections.</param>
        public FleckWebSocketServerAdapter(int? port = null, X509Certificate2 certificate = null)
        {
            if (port < 0)
            {
                throw new ArgumentException("Port cannot be negative", nameof(port));
            }

            // Certificate
            if (certificate == null)
            {
                this.IsSecure = false;
            }
            else
            {
                if (!certificate.Verify())
                {
                    throw new ArgumentException("The provided certificate is not valid", nameof(certificate));
                }

                this.Certificate = certificate;
                this.IsSecure = true;
            }

            // Port
            if (port.HasValue)
            {
                this.Port = port.Value;
            }
            else
            {
                this.Port = this.IsSecure ? DefaultPortSecured : DefaultPortUnsecured;
            }

            // Scheme
            this.WebSocketScheme = this.IsSecure ? WebSocketSchemeSecured : WebSocketSchemeUnsecured;

            // Logging
            FleckImpl.FleckLog.LogAction = (FleckImpl.LogLevel level, string message, Exception ex) =>
            {
                LogLevel? logLevel = null;
                switch (level)
                {
                    case FleckImpl.LogLevel.Error:
                        logLevel = LogLevel.Error;
                        break;
                    case FleckImpl.LogLevel.Warn:
                        logLevel = LogLevel.Warning;
                        break;
                    case FleckImpl.LogLevel.Info:
                        //logLevel = LogLevel.Info;
                        break;
                    default:
                        //logLevel = LogLevel.Debug;
                        break;
                }

                if (logLevel.HasValue)
                {
                    this.Logger?.Log(logLevel.Value, $"[Fleck] {message}", ex);
                }
            };
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public async Task<IWebSocketServer> Start()
        {
            Logger?.Log(LogLevel.Debug, $"Starting server at '{this.Location}'");

            string alreadyStartedMessage = $"Server at '{this.Location}' is already running";

            // Check if the server is already running
            if (this.IsRunning)
            {
                Logger?.Log(LogLevel.Debug, alreadyStartedMessage);

                // Return this object so calls can be chained
                return this;
            }

            // Start the server in a new thread
            return await Task.Run<IWebSocketServer>(() =>
            {
                lock (this._lock)
                {
                    // Check again if the server is already running in case it was started before reaching the lock statement
                    if (this.IsRunning)
                    {
                        Logger?.Log(LogLevel.Debug, alreadyStartedMessage);
                        return this;
                    }

                    // Try to start the server
                    try
                    {
                        // Create a new instance of the server
                        this._server = new FleckImpl.WebSocketServer(this.Location);

                        // Set the cert if required
                        if (this.Certificate != null)
                        {
                            this._server.Certificate = this.Certificate;
                        }

                        // Start the server
                        this._server.Start(FleckServiceConfig);
                        Logger?.Log(LogLevel.Warning, $"Started server at '{this.Location}'");
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            this._server.Dispose();
                        }
                        finally
                        {
                            this._server = null;
                            Logger?.Log(LogLevel.Error, $"Failed to start server at '{this.Location}'", ex);
                        }
                    }

                    // Return this object so calls can be chained
                    return this;
                }
            });
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public async Task<IWebSocketServer> Stop()
        {
            Logger?.Log(LogLevel.Debug, $"Stopping server at '{this.Location}'");

            string alreadyStoppedMessage = $"Server at '{this.Location}' is already stopped";

            // Check if the server is already stopped
            if (!this.IsRunning)
            {
                Logger?.Log(LogLevel.Debug, alreadyStoppedMessage);
                return this;
            }

            // Stop the server in a new thread
            return await Task.Run<IWebSocketServer>(() =>
            {
                lock (this._lock)
                {
                    // Check again if the server is already stopped in case it was stopped before reaching the lock statement
                    if (!this.IsRunning)
                    {
                        Logger?.Log(LogLevel.Debug, alreadyStoppedMessage);
                        return this;
                    }

                    // Try to stop the server
                    try
                    {
                        this._server.Dispose();
                        Logger?.Log(LogLevel.Info, $"Stopped server at '{this.Location}'");
                    }
                    catch (Exception ex)
                    {
                        Logger?.Log(LogLevel.Error, $"Failed to stop server at '{this.Location}'", ex);
                    }

                    // Return this object so calls can be chained
                    return this;
                }
            });
        }

        private void FleckServiceConfig(FleckImpl.IWebSocketConnection socket)
        {
            // Throw an exception if the "OnOpen" method has not been defined, so that clients cannot connect without validation
            socket.OnOpen = () =>
            {
                // Since delegates are immutable, assign the delegate to a variable to make sure that the value doesn't change
                // in the middle of the function (i.e. ensure thread-safety and avoid locking)
                var onOpenFunc = this.OnOpen;
                if (onOpenFunc == null)
                {
                    throw new NullReferenceException($"The {nameof(OnOpen)}() callback must be set in order to accept connections");
                }

                onOpenFunc(new FleckWebSocketConnection(socket), FleckWebSocketConnection.CreateConnectionInfo(socket));
            };
        }
    }
}
