namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;

    public abstract class WebSocketClientProxy
    {
        public string Id { get; }

        public Logger Logger { get; set; } = Logger.Default;

        private readonly WebSocketProxyActions _proxyActions;

        public WebSocketClientProxy(string id, WebSocketProxyActions proxyActions)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this._proxyActions = proxyActions ?? throw new ArgumentNullException(nameof(proxyActions));
        }

        public async Task SendMessage(Message message)
        {
            this.Logger?.Log(LogLevel.Debug, $"Sending message{(message.Id == null ? string.Empty : $" '{message.Id}'")} to client with ID: {this.Id}", message);
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
                this.Logger?.Log(LogLevel.Warning, $"Failed to send message to client with ID: {this.Id}", ex);
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
                this.Logger?.Log(LogLevel.Warning, $"Failed to gracefully close connection to client with ID: {this.Id}", ex);
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
                this.Logger?.Log(LogLevel.Error, $"Failed to determine whether the connection is open to client with ID: {this.Id}", ex);
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
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle new open connection to client with ID: {this.Id}", ex);
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
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle closing connection to client with ID: {this.Id}", ex);
            }
        }

        internal void OnErrorInternal(Exception error)
        {
            this.Logger?.Log(LogLevel.Error, $"Error in connection to client with ID: {this.Id}", error);
            try
            {
                this.OnError(error);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle error in connection to client with ID: {this.Id}", ex);
            }
        }

        internal void OnMessageInternal(string stringMessage)
        {
            this.Logger?.Log<string>(LogLevel.Debug, $"Message received from client with ID: {this.Id}", stringMessage);
            Message message = null;
            try
            {
                message = Message.FromJsonString(stringMessage);
                this.OnMessage(message);
            }
            catch (Exception ex)
            {
                this.Logger?.Log(LogLevel.Warning, $"Failed to handle message from client with ID: {this.Id}", ex);
                
                Request failedRequest = message == null
                    ? failedRequest = new Request(
                        null,
                        WellKnownMessageTypes.Unknown.ToString(),
                        stringMessage)
                    : failedRequest = new Request(
                        message.Id,
                        message.MessageType ?? WellKnownMessageTypes.Unknown.ToString(),
                        message.Body);

                ErrorResponse errorResponse = new ErrorResponse(
                    error: ex,
                    request: failedRequest,
                    includeStackTraceAndExceptionType: false);

                this.SendMessage(errorResponse).ContinueWith(sendTask =>
                {
                    this.Logger?.Log(LogLevel.Warning, $"Failed to send error to client with ID: {this.Id}", sendTask.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
