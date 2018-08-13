namespace Test
{
    using System;
    using System.Threading.Tasks;
    using RoRamu.Utils.Logging;
    using RoRamu.WebSocket;
    using RoRamu.WebSocket.Client;
    using RoRamu.WebSocket.Server;
    using RoRamu.WebSocket.Service;

    public class Program
    {
        private static readonly Logger Logger = Logger.Default;

        static void Main(string[] args)
        {
            Logger.LogExtraInfo = true;
            Logger.LogLevel = LogLevel.Info;

            var service = new TestService();
            service.Start().Wait();
            TestClientProxy.DefaultRequestTimeout = TimeSpan.FromSeconds(5);

            TestClient client = new TestClient(new WebSocketConnectionInfo("ws://localhost:80"));
            client.Connect().Wait();
            client.SendRequest(new Request("test", "this is a test"), TimeSpan.FromSeconds(10)).ContinueWith(requestTask =>
            {
                if (requestTask.IsFaulted)
                {
                    Logger?.Log(LogLevel.Error, "Failed to send test request", requestTask.Exception);
                }
                else
                {
                    RequestResult result = requestTask.Result;
                    if (result.IsSuccessful)
                    {
                        Logger?.Log(LogLevel.Info, "Received response to test message", result.Response);
                    }
                    else
                    {
                        Logger?.Log(LogLevel.Error, "Error when sending test request", result.Exception);
                    }
                }
            });

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }

                foreach (TestClientProxy proxy in service.Connections.Values)
                {
                    //var proxy = client;
                    Request request = new Request("test_request", new
                    {
                        TestRequestMessage = input,
                    });

                    client.SendRequest(request).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Logger?.Log(LogLevel.Error, $"Request '{request.Id}' to failed", task.Exception);
                        }
                        else
                        {
                            RequestResult requestResult = task.Result;
                            if (requestResult.IsSuccessful)
                            {
                                Logger?.Log(LogLevel.Info, $"Request '{request.Id}' succeeded!", requestResult.Response);
                            }
                            else
                            {
                                object extraInfo = requestResult.Exception ?? ((object)requestResult.Response);
                                Logger?.Log(LogLevel.Warning, $"Request '{request.Id}' received an error", extraInfo);
                            }
                        }
                    });
                }
            }

            client.Close().Wait();
            service.Stop().Wait();
        }

        private class TestClient : WebSocketClient
        {
            public TestClient(WebSocketConnectionInfo connectionInfo) : base(new WebSocket4NetConnection(connectionInfo))
            {
            }

            public new async Task SendMessage(Message message)
            {
                Logger?.Log(LogLevel.Info, $"Sending message", message);
                await base.SendMessage(message);
            }

            public new async Task<RequestResult> SendRequest(Request request, TimeSpan? requestTimeout = null)
            {
                Logger?.Log(LogLevel.Info, $"Sending request '{request.Id}'", request);
                return await base.SendRequest(request);
            }
        }

        private class TestService : WebSocketService<TestClientProxy>
        {
            public TestService() : base(new FleckWebSocketServerAdapter())
            {
            }

            protected override TestClientProxy CreateProxy(WebSocketConnectionInfo connectionInfo, WebSocketActions proxyActions)
            {
                return new TestClientProxy(Guid.NewGuid().ToString(), proxyActions);
            }
        }

        private class TestClientProxy : WebSocketClientProxy
        {
            public TestClientProxy(string id, WebSocketActions proxyActions) : base(id, proxyActions)
            {
            }

            public override async void OnMessage(Message message)
            {
                await this.SendMessage(Response.Create(message, new
                {
                    ThisIs = "A response",
                    RequestBody = message,
                }));
            }

            public new async Task SendMessage(Message message)
            {
                await base.SendMessage(message);
            }

            public new async Task<RequestResult> SendRequest(Request request, TimeSpan? requestTimeout = null)
            {
                return await base.SendRequest(request);
            }
        }
    }
}
