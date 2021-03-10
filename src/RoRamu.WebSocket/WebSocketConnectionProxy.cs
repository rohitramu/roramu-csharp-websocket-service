namespace RoRamu.WebSocket
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using RoRamu.Utils;
    using RoRamu.Utils.Logging;
    using RoRamu.Utils.Messaging;

    /// <summary>
    /// Represents a websocket connection and exposes methods for interacting with it.
    /// </summary>
    public abstract class WebSocketConnectionProxy : IWebSocketConnection
    {
        /// <summary>
        /// The logger to be used.
        /// Defaults to <see cref="RoRamu.Utils.Logging.Logger.Default" />.
        /// If <c>null</c>, no logs will be emitted.
        /// </summary>
        public Logger Logger { get; set; } = Logger.Default;

        /// <summary>
        /// The default request timeout.  This is initialized to 1 minute.
        /// </summary>
        public TimeSpan RequestTimeout
        {
            get => _requestTimeout;
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The default request timeout must be set to a positive value greater than zero");
                }

                _requestTimeout = value;
            }
        }
        private TimeSpan _requestTimeout = TimeSpan.FromMinutes(1);

        private readonly WebSocketUnderlyingConnection _connection;

        private readonly WebSocketController _controller;

        private event Action<Response> ReceivedResponse;

        /// <summary>
        /// Creates a new websocket connection proxy.
        /// </summary>
        /// <param name="connection">
        /// The underlying websocket implementation's connection object.
        /// </param>
        /// <param name="controllerFactory">
        /// A factory method which creates a controller containing the desired behavior for this connection.
        /// </param>
        public WebSocketConnectionProxy(WebSocketUnderlyingConnection connection, WebSocketController.FactoryDelegate controllerFactory)
        {
            this._connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (controllerFactory == null)
            {
                throw new ArgumentNullException(nameof(controllerFactory));
            }
            WebSocketController controller = controllerFactory(this);
            this._controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        /// <inheritdoc />
        public async Task SendMessage(Message message)
        {
            string messageIdLogString = message.Id == null ? string.Empty : $" '{message.Id}'";
            this.Logger?.Log(LogLevel.Debug, $"Sending message{messageIdLogString}", message);
            try
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }
                await this._connection.SendMessage(message.ToJsonString());
                this.Logger?.Log(LogLevel.Info, $"Sent message{messageIdLogString}", message);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to send message{messageIdLogString}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<RequestResult> SendRequest(Request request, TimeSpan? requestTimeout = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // If a timeout isn't specified, use the current default
            TimeSpan timeout = requestTimeout ?? this.RequestTimeout;

            // Create a task completion source so that we can wait on the event to fire
            TaskCompletionSource<RequestResult> resultTaskContainer = new TaskCompletionSource<RequestResult>();

            // Create a cancellation token source so that we can enforce a timeout
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);

            // Register a callback with the cancellation token to cancel the waiting task on a timeout
            cancellationTokenSource.Token.Register(
                () => resultTaskContainer.TrySetException(new TimeoutException($"No response received for request '{request.Id}' after waiting for {timeout.ToFormattedString()}")),
                useSynchronizationContext: false);

            // Define a response listener for this request's ID
            void handleResponseFunc(Response response)
            {
                if (response.Id == request.Id)
                {
                    // Unregister the response listener first in case the event handler gets called twice
                    this.ReceivedResponse -= handleResponseFunc;

                    // Set the result
                    resultTaskContainer.SetResult(RequestResult.Success(request, response));
                }
            }

            // Register the response listener
            this.ReceivedResponse += handleResponseFunc;

            // Try to send the message
            try
            {
                await this.SendMessage(request);
                return await resultTaskContainer.Task;
            }
            catch (Exception ex)
            {
                // Unregister the response listener
                this.ReceivedResponse -= handleResponseFunc;

                // Return the exception as the failure result
                return RequestResult.Failure(request, ex);
            }
        }

        /// <inheritdoc />
        public bool IsOpen()
        {
            try
            {
                return this._connection.IsOpen();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, $"Failed to determine whether the connection is open", ex);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task Close()
        {
            this.Logger?.Log(LogLevel.Debug, $"Closing connection", this);
            try
            {
                await this._connection.Close();
                this.Logger?.Log(LogLevel.Info, $"Connection closed", this);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to gracefully close connection", ex);
            }
        }

        internal async Task OnOpenInternal()
        {
            this.Logger?.Log(LogLevel.Info, $"Connection opened", this);
            try
            {
                await this._controller.OnOpen();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle new open connection", ex);
            }
        }

        internal async Task OnCloseInternal()
        {
            this.Logger?.Log(LogLevel.Info, $"Connection closed", this);
            try
            {
                await this._controller.OnClose();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle closing connection", ex);
            }
        }

        internal async Task OnErrorInternal(Exception error)
        {
            this.Logger?.Log(LogLevel.Info, $"Error in connection", error);
            try
            {
                await this._controller.OnError(error);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle error in connection", ex);
            }
        }

        internal async Task OnMessageInternal(string stringMessage)
        {
            this.Logger?.Log<string>(LogLevel.Info, $"Message received", stringMessage);
            Message message = null;
            try
            {
                message = Message.FromJsonString(stringMessage);
                if (message.TryParseResponse(out Response response))
                {
                    ReceivedResponse?.Invoke(response);
                }
                else
                {
                    await this._controller.OnMessage(message);
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle message", ex);

                string messageId = message?.Id;
                ErrorResponse errorResponse = new ErrorResponse(
                    error: ex,
                    requestId: messageId,
                    includeStackTrace: false);

                await this.SendMessage(errorResponse).ContinueWith(sendTask =>
                {
                    this.Logger?.Log(LogLevel.Warning, $"Failed to send error", sendTask.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
