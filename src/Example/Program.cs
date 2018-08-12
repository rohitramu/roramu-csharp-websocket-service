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
        private static readonly Logger Logger = Logger.Default;

        static void Main(string[] args)
        {
            Logger.LogExtraInfo = true;
            Logger.LogLevel = LogLevel.Debug;

            var service = new TestService(new FleckWebSocketServerAdapter());
            Task serviceTask = service.Start();
            TestProxy.DefaultRequestTimeout = TimeSpan.FromSeconds(15);
            
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }

                foreach (TestProxy proxy in service.Connections.Values)
                {
                    Request request = new Request("test_request", new
                    {
                        TestRequestMessage = input,
                        FixedMessage = "This is a request!",
                    });

                    proxy.SendRequest(request).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Logger?.Log(LogLevel.Error, $"Request '{request.Id}' to client '{proxy.Id}' failed", task.Exception);
                        }
                        else
                        {
                            RequestResult requestResult = task.Result;
                            if (requestResult.IsSuccessful)
                            {
                                Logger?.Log(LogLevel.Info, $"Request '{request.Id}' to client '{proxy.Id}' succeeded!", requestResult.Response.Body);
                            }
                            else
                            {
                                object extraInfo = requestResult.Exception ?? requestResult.Response.Body;
                                Logger?.Log(LogLevel.Warning, $"Request '{request.Id}' to client '{proxy.Id}' received an error", extraInfo);
                            }
                        }
                    });
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
                return new TestProxy(Guid.NewGuid().ToString(), proxyActions);
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

            public override async void OnOpen()
            {
                await this.SendMessage(new Message(null, "TestMessage", new
                {
                    MyMessage = "Hello, World!",
                    ClientId = this.Id,
                }));
            }
        }
    }
}
