namespace RoRamu.WebSocket.Server
{
    using System.Security.Cryptography.X509Certificates;

    public sealed class WebSocketServerCommandLine
    {
        public int? Port { get; set; } = null;

        public X509Certificate2 Certificate { get; set; } = null;

        public static WebSocketServerCommandLine ParseCommandLineArgs(string[] args)
        {
            // Server port
            int? port = null;
            if (args.Length > 0)
            {
                port = int.Parse(args[0]);
            }

            // SSL cert
            X509Certificate2 certificate = null;
            if (args.Length > 1)
            {
                string certLocation = args[1];

                string password = null;
                if (args.Length > 2)
                {
                    password = args[2];
                    certificate = new X509Certificate2(certLocation, password);
                }
                else
                {
                    certificate = new X509Certificate2(certLocation);
                }
            }

            return new WebSocketServerCommandLine()
            {
                Port = port,
                Certificate = certificate,
            };
        }
    }
}
