namespace RoRamu.WebSocket
{
    using System;

    public class WebSocketActions
    {
        public IsOpenDelegate IsOpen { get; }

        public SendMessageDelegate SendMessage { get; }

        public CloseDelegate Close { get; }

        public WebSocketActions(
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
