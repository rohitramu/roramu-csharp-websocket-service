namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;

    public sealed class FleckWebSocketServerAdapter : IWebSocketServer
    {
        public Action<WebSocketConnection, WebSocketConnectionInfo> OnOpen { get; set; }

        public const int DefaultPortSecured = 443;
        public const int DefaultPortUnsecured = 80;

        public const string WebSocketSchemeSecured = "wss";
        public const string WebSocketSchemeUnsecured = "ws";

        public Logger Logger { get; set; } = Logger.Default;

        public int Port { get; }

        public X509Certificate2 Certificate { get; }

        public bool IsSecure { get; }

        public bool IsRunning => this._server != null;

        private string WebSocketScheme { get; }

        private string Location => $"{this.WebSocketScheme}://0.0.0.0:{this.Port}";

        private Fleck.WebSocketServer _server;

        private readonly object _lock = new object();

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
            Fleck.FleckLog.LogAction = (Fleck.LogLevel level, string message, Exception ex) =>
            {
                LogLevel? logLevel = null;
                switch (level)
                {
                    case Fleck.LogLevel.Error:
                        logLevel = LogLevel.Error;
                        break;
                    case Fleck.LogLevel.Warn:
                        logLevel = LogLevel.Warning;
                        break;
                    case Fleck.LogLevel.Info:
                        //logLevel = LogLevel.Info;
                        break;
                    default:
                        //logLevel = LogLevel.Debug;
                        break;
                }

                if (logLevel.HasValue)
                {
                    this.Logger?.Log(logLevel.Value, message, ex);
                }
            };
        }

        public async Task Start()
        {
            Logger?.Log(LogLevel.Debug, $"Starting server at '{this.Location}'");

            string alreadyStartedMessage = $"Server at '{this.Location}' is already running";

            // Check if the server is already running
            if (this.IsRunning)
            {
                Logger?.Log(LogLevel.Debug, alreadyStartedMessage);
                return;
            }

            // Start the server in a new thread
            await Task.Run(() =>
            {
                lock (this._lock)
                {
                    // Check again if the server is already running in case it was started before reaching the lock statement
                    if (this.IsRunning)
                    {
                        Logger?.Log(LogLevel.Debug, alreadyStartedMessage);
                        return;
                    }

                    // Try to start the server
                    try
                    {
                        // Create a new instance of the server
                        this._server = new Fleck.WebSocketServer(this.Location);

                        // Set the cert if required
                        if (this.Certificate != null)
                        {
                            this._server.Certificate = this.Certificate;
                        }

                        // Start the server
                        this._server.Start(FleckServiceConfig);
                        Logger?.Log(LogLevel.Info, $"Started server at '{this.Location}'");
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
                }
            });
        }

        public async Task Stop()
        {
            Logger?.Log(LogLevel.Debug, $"Stopping server at '{this.Location}'");

            string alreadyStoppedMessage = $"Server at '{this.Location}' is already stopped";

            // Check if the server is already stopped
            if (!this.IsRunning)
            {
                Logger?.Log(LogLevel.Debug, alreadyStoppedMessage);
                return;
            }

            // Stop the server in a new thread
            await Task.Run(() =>
            {
                lock (this._lock)
                {
                    // Check again if the server is already stopped in case it was stopped before reaching the lock statement
                    if (!this.IsRunning)
                    {
                        Logger?.Log(LogLevel.Debug, alreadyStoppedMessage);
                        return;
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
                }
            });
        }

        private void FleckServiceConfig(Fleck.IWebSocketConnection socket)
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
