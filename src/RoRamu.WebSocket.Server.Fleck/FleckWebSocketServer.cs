namespace RoRamu.WebSocket.Server
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.Utils;

    public sealed class FleckWebSocketServer : IWebSocketServer
    {
        public Action<IWebSocket> OnOpen { get; set; }

        public const int DefaultPortSecured = 443;
        public const int DefaultPortUnsecured = 80;

        public const string WebSocketSchemeSecured = "wss";
        public const string WebSocketSchemeUnsecured = "ws";

        public Logger Logger { get; set; } = Logger.DefaultLogger;

        public int Port { get; }

        public X509Certificate2 Certificate { get; }

        public bool IsSecure { get; }

        public bool IsRunning { get; private set; } = false;

        private string WebSocketScheme { get; }

        private string Location => $"{this.WebSocketScheme}://0.0.0.0:{this.Port}";

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public FleckWebSocketServer(int? port = null, X509Certificate2 certificate = null)
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
                if (this.Logger != null)
                {
                    switch (level)
                    {
                        case Fleck.LogLevel.Debug:
                            Logger.Log(LogLevel.Debug, message);
                            break;
                        case Fleck.LogLevel.Error:
                            Logger.Log(LogLevel.Error, message, ex);
                            break;
                        case Fleck.LogLevel.Warn:
                            Logger.Log(LogLevel.Warning, message);
                            break;
                        default:
                            Logger.Log(LogLevel.Info, message);
                            break;
                    }
                }
            };
            Fleck.FleckLog.Level = Fleck.LogLevel.Warn;
        }

        private readonly object _startLock = new object();
        public async Task Start()
        {
            // Check if the service is already running
            if (this.IsRunning)
            {
                return;
            }

            // Start the service in a new thread
            await Task.Run(() =>
            {
                lock (this._startLock)
                {
                    // Check again if the service is already running in case it was started before reaching the lock statement
                    if (this.IsRunning)
                    {
                        return;
                    }

                    using (Fleck.WebSocketServer service = new Fleck.WebSocketServer(this.Location))
                    {
                        // Set the cert if required
                        if (this.Certificate != null)
                        {
                            service.Certificate = this.Certificate;
                        }

                        // Start the service
                        service.Start(FleckServiceConfig);

                        // Mark this service as running
                        this.IsRunning = true;

                        // Wait for the cancellation token to tell the server to stop
                        this._cancellationTokenSource.Token.WaitHandle.WaitOne();
                    }
                }
            },
            this._cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (!this._cancellationTokenSource.IsCancellationRequested)
            {
                this._cancellationTokenSource.Cancel();
            }
        }

        private void FleckServiceConfig(Fleck.IWebSocketConnection socket)
        {
            socket.OnOpen = () => this.OnOpen?.Invoke(new FleckWebSocketProxy(socket));
        }

        private class FleckWebSocketProxy : IWebSocket
        {
            public Action OnClose { get; set; }
            public Action<Exception> OnError { get; set; }
            public Action<string> OnMessage { get; set; }

            public const int MaxTextMessageLength = 1024 * 8;

            public bool IsOpen => this._socket.IsAvailable;

            public IReadOnlyDictionary<string, string> Headers { get; }

            public IReadOnlyDictionary<string, string> Cookies { get; }

            public string ClientIpAddress { get; }

            private readonly Fleck.IWebSocketConnection _socket;

            public FleckWebSocketProxy(Fleck.IWebSocketConnection socket)
            {
                this._socket = socket ?? throw new ArgumentNullException(nameof(socket));
                this.Headers = new Dictionary<string, string>(socket.ConnectionInfo.Headers);
                this.Cookies = new Dictionary<string, string>(socket.ConnectionInfo.Cookies);
                this.ClientIpAddress = socket.ConnectionInfo.ClientIpAddress;

                socket.OnMessage = message => this.OnMessage?.Invoke(message);
                socket.OnBinary = data => this.OnMessage?.Invoke(data.DecodeToString());
                socket.OnError = ex => this.OnError?.Invoke(ex);
                socket.OnClose = () => this.OnClose?.Invoke();
            }

            public async Task Close()
            {
                await Task.Run(() => this._socket.Close());
            }

            public async Task SendMessage(string message)
            {
                if (message.Length <= MaxTextMessageLength)
                {
                    await this._socket.Send(message);
                }
                else
                {
                    await this._socket.Send(message.Encode());
                }
            }
        }
    }
}
