namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.WebSocket.Server;
    using RoRamu.WebSocket.Service;

    public class Program
    {
        static void Main(string[] args)
        {
            Logger.LogExtraInfo = false;
            Logger.LogLevel = LogLevel.Info;

            var service = new TestService(new FleckWebSocketServer());

            Task serviceTask = service.Start();
            
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }
                service.Broadcast(input).Wait();
            }

            service.Stop();
            serviceTask.Wait();
        }

        private class TestService : WebSocketService<TestProxy>
        {
            public TestService(IWebSocketServer server) : base(server)
            {
            }

            protected override TestProxy CreateProxy(
                string clientIpAddress,
                IReadOnlyDictionary<string, string> headers,
                IReadOnlyDictionary<string, string> cookies,
                IsOpen isOpenFunc,
                SendMessage sendMessageFunc,
                Close closeFunc)
            {
                return new TestProxy(
                    "test ID",
                    isOpenFunc,
                    sendMessageFunc,
                    closeFunc);
            }
        }

        private class TestProxy : WebSocketProxy
        {
            public TestProxy(
                string id,
                IsOpen isOpenFunc,
                SendMessage sendMessageFunc,
                Close closeFunc) : base(id, isOpenFunc, sendMessageFunc, closeFunc)
            {

            }

            public override async void OnMessage(string message)
            {
                base.OnMessage(message);
                await this.SendMessage(message);
            }
        }
    }
}
