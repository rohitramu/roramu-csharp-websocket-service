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
            Logger.LogLevel = LogLevel.Debug;

            var service = new TestService(new FleckWebSocketServerAdapter());

            Task serviceTask = service.Start();
            
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }

                foreach (TestProxy proxy in service.Connections.Values)
                {
                    proxy.SendMessage(input).Start();
                }
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
                WebSocketProxyActions proxyActions)
            {
                return new TestProxy("test ID", proxyActions);
            }
        }

        private class TestProxy : WebSocketClientProxy
        {
            public TestProxy(string id, WebSocketProxyActions proxyActions) : base(id, proxyActions)
            {
            }

            public override async void OnMessage(Message message)
            {
                await this.SendMessage(message);
            }

            public async Task SendMessage(string message)
            {
                await base.SendMessage(new Request("TestMessage", message));
            }
        }
    }
}
