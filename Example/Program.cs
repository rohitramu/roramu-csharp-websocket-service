namespace Test
{
    using System;
    using RoRamu.Utils.Logging;
    using RoRamu.Utils.Messaging;
    using RoRamu.WebSocket;
    using RoRamu.WebSocket.Service;

    public class Program
    {
        private static readonly Logger Logger = Logger.Default;

        static void Main()
        {
            Logger.LogExtraInfo = true;
            Logger.LogLevel = LogLevel.Info;

            var service = new TestService();
            service.Start().Wait();

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

                foreach (WebSocketClientProxy proxy in service.Connections.Values)
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
    }
}
