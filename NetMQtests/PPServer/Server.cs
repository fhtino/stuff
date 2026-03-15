using NetMQ;
using NetMQ.Sockets;
using System.ServiceModel.Security;

namespace PPServer
{
    internal class Server
    {
        public static void Main(string[] args)
        {
            int port = 7777;

            bool useEncryption = args.Contains("--encryption", StringComparer.OrdinalIgnoreCase);

            Console.WriteLine($" - Starting server on port {port}");
            var server = new PullSocket();

            if (useEncryption)
            {
                Console.WriteLine(" - Using Curve encryption.");
                string publicKeyBase64 = File.ReadAllText("../../../../cert_public.key");
                string secretKeyBase64 = File.ReadAllText("../../../../cert_secret.key");
                Console.WriteLine($" - Server public key: {publicKeyBase64}");
                var serverSertificate = new NetMQCertificate(Convert.FromBase64String(secretKeyBase64),
                                                             Convert.FromBase64String(publicKeyBase64));
                server.Options.CurveServer = true;
                server.Options.CurveCertificate = serverSertificate;
            }

            server.Bind($"tcp://*:{port}");
            var poller = new NetMQPoller { server };
            server.ReceiveReady += (sender, args) =>
            {
                var message = args.Socket.ReceiveFrameString();
                Console.WriteLine($"Received message: [{message}]");
            };

            poller.RunAsync("ServerPoller", isBackgroundThread: true);

            Console.WriteLine(" - Server is running.\n - Press any key to exit.\n");
            Console.ReadKey();
        }

    }

}
