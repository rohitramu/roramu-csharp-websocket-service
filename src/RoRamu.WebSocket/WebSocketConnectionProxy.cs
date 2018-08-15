namespace RoRamu.WebSocket
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using RoRamu.Utils;
    using RoRamu.Utils.Logging;

    public abstract class WebSocketConnectionProxy
    {
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

        private readonly WebSocketActions _proxyActions;

        private event Action<Response> ReceivedResponse;

        public WebSocketConnectionProxy(WebSocketActions proxyActions)
        {
            this._proxyActions = proxyActions ?? throw new ArgumentNullException(nameof(proxyActions));
        }

        protected async Task SendMessage(Message message)
        {
            string messageIdLogString = message.Id == null ? string.Empty : $" '{message.Id}'";
            this.Logger?.Log(LogLevel.Debug, $"Sending message{messageIdLogString}", message);
            try
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }
                await this._proxyActions.SendMessage(message);
                this.Logger?.Log(LogLevel.Info, $"Sent message{messageIdLogString}", message);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to send message{messageIdLogString}", ex);
            }
        }

        /// <summary>
        /// Sends a request and then waits for the given timeout for a response.  If the timeout is hit, this method will throw a <see cref="TimeoutException"/>.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="timeout">The timeout to use for this request - leave as null to use the default timeout (<see cref="WebSocketConnectionProxy.DefaultRequestTimeout"/>)</param>
        /// <returns>The response</returns>
        protected async Task<RequestResult> SendRequest(Request request, TimeSpan? requestTimeout = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // If a timeout isn't specified, use the current default
            TimeSpan timeout = requestTimeout ?? WebSocketConnectionProxy.DefaultRequestTimeout;

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

        public async Task Close()
        {
            this.Logger?.Log(LogLevel.Debug, $"Closing connection", this);
            try
            {
                await this._proxyActions.Close();
                this.Logger?.Log(LogLevel.Info, $"Connection closed", this);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to gracefully close connection", ex);
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
                this.Logger?.Log(LogLevel.Error, $"Failed to determine whether the connection is open", ex);
                return false;
            }
        }

        public virtual void OnOpen() { }

        public virtual void OnClose() { }

        public virtual void OnError(Exception error) { }

        public virtual void OnMessage(Message message) { }

        protected void OnOpenInternal()
        {
            this.Logger?.Log(LogLevel.Info, $"Connection opened", this);
            try
            {
                this.OnOpen();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle new open connection", ex);
            }
        }

        protected void OnCloseInternal()
        {
            this.Logger?.Log(LogLevel.Info, $"Connection closed", this);
            try
            {
                this.OnClose();
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle closing connection", ex);
            }
        }

        protected void OnErrorInternal(Exception error)
        {
            this.Logger?.Log(LogLevel.Info, $"Error in connection", error);
            try
            {
                this.OnError(error);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle error in connection", ex);
            }
        }

        protected void OnMessageInternal(string stringMessage)
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
                    this.OnMessage(message);
                }
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle message", ex);

                string messageId = message?.Id;
                ErrorResponse errorResponse = new ErrorResponse(
                    error: ex,
                    requestId: messageId,
                    includeDebugInfo: false);

                this.SendMessage(errorResponse).ContinueWith(sendTask =>
                {
                    this.Logger?.Log(LogLevel.Warning, $"Failed to send error", sendTask.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
