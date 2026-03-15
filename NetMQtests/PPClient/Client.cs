using NetMQ;
using NetMQ.Sockets;

namespace PPClient
{
    internal class Client
    {
        public static void Main(string[] args)
        {
            int tcpPort = 7777;

            bool useEncryption = args.Contains("--encryption", StringComparer.OrdinalIgnoreCase);
            string serverIP = args.FirstOrDefault(arg => arg.StartsWith("--server"))?.Split("=")[1] ?? "localhost";
            int exitAfter = int.Parse(args.FirstOrDefault(arg => arg.StartsWith("--exitafter"))?.Split("=")[1] ?? "5");

            Console.WriteLine($" - Connecting to server {serverIP} {tcpPort}");
            var client = new PushSocket();

            if (useEncryption)
            {
                // set the client certificate
                var clientCertificate = new NetMQCertificate();
                client.Options.CurveCertificate = clientCertificate;

                // set the server public key
                byte[] serverPublicKey = Convert.FromBase64String(File.ReadAllText("../../../../cert_public.key"));
                client.Options.CurveServerKey = serverPublicKey!;
                // it's the same as setting CurveServerKey, but more explicit
                //client.Options.CurveServerCertificate = new NetMQCertificate(new byte[32], serverPublicKey);  

                Console.WriteLine($" - Using Curve encryption:");
                Console.WriteLine($" - Client public key: {Convert.ToBase64String(clientCertificate.PublicKey)}");
                Console.WriteLine($" - Server public key: {Convert.ToBase64String(serverPublicKey)}");
            }

            client.Options.SendHighWatermark = 100;  // to limit the number of messages in the output queue
            client.Connect($"tcp://{serverIP}:{tcpPort}");

            int messageCounter = 0;
            int failCounter = 0;
            while (true)
            {
                var message = $"BODY_{DateTime.UtcNow.ToString("O")}_{messageCounter++}";
                Console.Write($"Sending message [{message}]...");
                //client.SendFrame(message); // blocking, if output queue is full
                bool sendResult = client.TrySendFrame(message);  // does not block, but you can lose messages if output queue is full
                Console.WriteLine($" sendResult={sendResult}");
                if (!sendResult) failCounter++;

                Thread.Sleep(1000);

                if (messageCounter >= exitAfter)
                    break;
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

    }

}
