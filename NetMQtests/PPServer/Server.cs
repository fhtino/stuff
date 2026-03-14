using NetMQ;
using NetMQ.Sockets;

namespace PPServer
{
    internal class Server
    {
        public static void Main(string[] args)
        {
            int port = 7777;
            Console.WriteLine($"Starting server on port {port}...");
            var server = new PullSocket();
            server.Bind($"tcp://*:{port}");
            var poller = new NetMQPoller { server };
            server.ReceiveReady += (sender, args) =>
            {
                var message = args.Socket.ReceiveFrameString();
                Console.WriteLine($"Received message: [{message}]");
            };
            poller.RunAsync("ServerPoller", isBackgroundThread:true);
            Console.WriteLine("Server is running.\nPress any key to exit.\n");
            Console.ReadKey();
        }
    }
}
