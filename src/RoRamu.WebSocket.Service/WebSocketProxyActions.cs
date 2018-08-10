namespace RoRamu.WebSocket.Service
{
    using System;

    public class WebSocketProxyActions
    {
        internal IsOpenDelegate IsOpen { get; }

        internal SendMessageDelegate SendMessage { get; }

        internal CloseDelegate Close { get; }

        public WebSocketProxyActions(
            IsOpenDelegate isOpenFunc,
            SendMessageDelegate sendMessageFunc,
            CloseDelegate closeFunc)
        {
            this.IsOpen = isOpenFunc ?? throw new ArgumentNullException(nameof(isOpenFunc));
            this.SendMessage = sendMessageFunc ?? throw new ArgumentNullException(nameof(sendMessageFunc));
            this.Close = closeFunc ?? throw new ArgumentNullException(nameof(closeFunc));
        }
    }
}
