namespace RoRamu.WebSocket.Service
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.Utils;

    public class WebSocketProxy : IWebSocketProxy
    {
        public string Id { get; }

        private readonly IsOpen _isOpenFunc;
        private readonly SendMessage _sendMessageFunc;
        private readonly Close _closeFunc;

        private readonly Logger _logger;

        public WebSocketProxy(
            string id,
            IsOpen isOpenFunc,
            SendMessage sendMessageFunc,
            Close closeFunc,
            Logger logger = null)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"{nameof(id)} cannot be empty or whitespace");
            }

            this._isOpenFunc = isOpenFunc ?? throw new ArgumentNullException(nameof(isOpenFunc));
            this._sendMessageFunc = sendMessageFunc ?? throw new ArgumentNullException(nameof(sendMessageFunc));
            this._closeFunc = closeFunc ?? throw new ArgumentNullException(nameof(closeFunc));
            this.Id = id;
            this._logger = logger ?? Logger.DefaultLogger;
        }

        public async Task SendMessage(string message)
        {
            await this._sendMessageFunc(message);
            this._logger?.Log(LogLevel.Info, "Message sent", message);
        }

        public async Task Close()
        {
            await this._closeFunc();
        }

        public void SendError(Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            string json = JsonUtils.SerializeToJson(new
            {
                ex.GetType().FullName,
                ex.Message,
                ex.Source,
            });
            this._sendMessageFunc(json);
        }

        public virtual void OnOpen()
        {
            this._logger?.Log(LogLevel.Info, $"Client '{this.Id}' connected");
        }

        public virtual void OnClose()
        {
            this._logger?.Log(LogLevel.Info, $"Client '{this.Id}' disconnected");
        }

        public virtual void OnError(Exception ex)
        {
            this._logger?.Log(LogLevel.Error, ex.Message, ex);
        }

        public virtual void OnMessage(string message)
        {
            this._logger?.Log(LogLevel.Info, "Message received", message);
        }
    }
}
