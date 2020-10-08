namespace RoRamu.WebSocket.Server
{
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// A command line parser for getting parameters for starting a websocket server.
    /// </summary>
    public sealed class WebSocketServerCommandLine
    {
        /// <summary>
        /// The port to listen on.
        /// </summary>
        public int? Port { get; private set; } = null;

        /// <summary>
        /// The SSL certificate to use when establishing connections.
        /// </summary>
        public X509Certificate2 Certificate { get; private set; } = null;

        /// <summary>
        /// Parses command line arguments to set the port and SSL certificate location when starting
        /// the server.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns></returns>
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
