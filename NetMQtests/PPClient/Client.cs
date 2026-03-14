using NetMQ;
using NetMQ.Sockets;

namespace PPClient
{
    internal class Client
    {
        public static void Main(string[] args)
        {
            string serverIP = args.Length> 0 ? args[0] : "localhost";

            Console.WriteLine($"Connecting to server at {serverIP}");

            var client = new PushSocket();
            client.Options.SendHighWatermark = 100;  // to limit the number of messages in the output queue
            client.Connect($"tcp://{serverIP}:7777");

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
                
                if (messageCounter >= 5)
                    break;                
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}
