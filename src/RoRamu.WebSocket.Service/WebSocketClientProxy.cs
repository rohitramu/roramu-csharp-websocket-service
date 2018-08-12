namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using RoRamu.Utils;
    using RoRamu.Utils.Logging;

    public abstract class WebSocketClientProxy
    {
        public string Id { get; }

        /// <summary>
        /// The default request timeout.  This is initialized to 1 minute.
        /// </summary>
        public static TimeSpan DefaultRequestTimeout
        {
            get => _defaultRequestTimeout;
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The default request timeout must be set to a positive value greater than zero");
                }

                _defaultRequestTimeout = value;
            }
        }
        private static TimeSpan _defaultRequestTimeout = TimeSpan.FromMinutes(1);

        public Logger Logger { get; set; } = Logger.Default;

        private readonly WebSocketProxyActions _proxyActions;

        private event Action<Response> ReceivedResponse;

        public WebSocketClientProxy(string id, WebSocketProxyActions proxyActions)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this._proxyActions = proxyActions ?? throw new ArgumentNullException(nameof(proxyActions));
        }

        public async Task SendMessage(Message message)
        {
            this.Logger?.Log(LogLevel.Debug, $"Sending message{(message.Id == null ? string.Empty : $" '{message.Id}'")} to client '{this.Id}'", message);
            try
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }
                await this._proxyActions.SendMessage(message);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to send message to client '{this.Id}'", ex);
            }
        }

        /// <summary>
        /// Sends a request and then waits for the given timeout for a response.  If the timeout is hit, this method will throw a <see cref="TimeoutException"/>.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="timeout">The timeout to use for this request - leave as null to use the default timeout (<see cref="WebSocketClientProxy.DefaultRequestTimeout"/>)</param>
        /// <returns>The response</returns>
        public async Task<RequestResult> SendRequest(Request request, TimeSpan? requestTimeout = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // If a timeout isn't specified, use the current default
            TimeSpan timeout = requestTimeout ?? WebSocketClientProxy.DefaultRequestTimeout;

            // Create a task completion source so that we can wait on the event to fire
            TaskCompletionSource<RequestResult> resultTaskContainer = new TaskCompletionSource<RequestResult>();

            // Create a cancellation token source so that we can enforce a timeout
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);

            // Register a callback with the cancellation token to cancel the waiting task on a timeout
            cancellationTokenSource.Token.Register(
                () => resultTaskContainer.TrySetException(new TimeoutException($"No response received for request '{request.Id}' to client '{this.Id}' after waiting for {timeout.ToFormattedString()}")),
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

        public async Task Close()
        {
            this.Logger?.Log(LogLevel.Debug, $"Closing connection to client '{this.Id}'");
            try
            {
                await this._proxyActions.Close();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to gracefully close connection to client '{this.Id}'", ex);
            }
        }

        public bool IsOpen()
        {
            try
            {
                return this._proxyActions.IsOpen();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Error, $"Failed to determine whether the connection is open to client '{this.Id}'", ex);
                return false;
            }
        }

        public virtual void OnOpen() { }

        public virtual void OnClose() { }

        public virtual void OnError(Exception error) { }

        public virtual void OnMessage(Message message) { }

        internal void OnOpenInternal()
        {
            this.Logger?.Log(LogLevel.Debug, $"Client '{this.Id}' connected");
            try
            {
                this.OnOpen();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle new open connection to client '{this.Id}'", ex);
            }
        }

        internal void OnCloseInternal()
        {
            this.Logger?.Log(LogLevel.Debug, $"Client '{this.Id}' disconnected");
            try
            {
                this.OnClose();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle closing connection to client '{this.Id}'", ex);
            }
        }

        internal void OnErrorInternal(Exception error)
        {
            this.Logger?.Log(LogLevel.Error, $"Error in connection to client '{this.Id}'", error);
            try
            {
                this.OnError(error);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle error in connection to client '{this.Id}'", ex);
            }
        }

        internal void OnMessageInternal(string stringMessage)
        {
            this.Logger?.Log<string>(LogLevel.Debug, $"Message received from client '{this.Id}'", stringMessage);
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
                    this.OnMessage(message);
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle message from client '{this.Id}'", ex);

                string messageId = message?.Id;
                ErrorResponse errorResponse = new ErrorResponse(
                    error: ex,
                    requestId: messageId,
                    includeDebugInfo: false);

                this.SendMessage(errorResponse).ContinueWith(sendTask =>
                {
                    this.Logger?.Log(LogLevel.Warning, $"Failed to send error to client '{this.Id}'", sendTask.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
