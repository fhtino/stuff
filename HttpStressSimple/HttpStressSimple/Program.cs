using System.Diagnostics;
using System.Reflection;

namespace HttpStressSimple
{
    internal class Program
    {

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"HttpStressSimple - ver. {Assembly.GetExecutingAssembly().GetName().Version}\n");

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            try
            {
                string url = args[0];
                int pauseMS = args.Length > 1 ? int.Parse(args[1]) : 1000;
                int numberCalls = args.Length > 2 ? int.Parse(args[2]) : 10;

                var sw = new Stopwatch();
                var httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });                

                for (int i = 0; i < numberCalls; i++)
                {
                    sw.Restart();
                    var resp = await httpClient.GetAsync(url);
                    byte[] body = await resp.Content.ReadAsByteArrayAsync();
                    Console.WriteLine($"{i} - {DateTime.UtcNow.ToString()} : Elapsed={(sw.Elapsed.TotalMilliseconds / 1000.0).ToString("0.000")} Code={(int)resp.StatusCode} Len={body.Length} ");
                    await Task.Delay(pauseMS);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public static void ShowUsage()
        {
            Console.WriteLine("");
            Console.WriteLine("HttpStressSimple.exe  http://yoursite.test [pause_ms] [number_of_calls]");
        }

    }
}
