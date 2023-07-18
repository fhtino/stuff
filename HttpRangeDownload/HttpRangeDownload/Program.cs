using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace HttpRangeDownload
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string url = args[0];
            Console.WriteLine(url);

            long minChunkSize = 20 * 1024;
            long maxChunkSize = 500 * 1024;
            long targetChunkTime = 5000;  // milliseconds
            long chunkSize = minChunkSize;
            long position = 0;
            long downloadedBytes = 0;

            using (var fileStream = File.Create("out.dat"))
            {
                var sw = new Stopwatch();

                var httpClient = new HttpClient();

                bool exitCycle = false;

                while (!exitCycle)
                {
                    sw.Restart();

                    //  Thread.Sleep(4500);   // slow down for testing

                    // Donwload data (note: output array contains chunkSize + 1 bytes)
                    httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(position, position + chunkSize);
                    var bytesFromDownload = httpClient.GetByteArrayAsync(url).Result;

                    // check exit condition
                    if (bytesFromDownload.Length <= chunkSize)
                    {
                        exitCycle = true;
                    }
                    else
                    {
                        Array.Resize(ref bytesFromDownload, (int)chunkSize);  // remove the extra 1 byte at the end
                    }

                    // dump to file
                    fileStream.Write(bytesFromDownload, 0, bytesFromDownload.Length);
                    downloadedBytes += bytesFromDownload.Length;

                    // calculate new position and chunkSize
                    position += chunkSize;
                    chunkSize = chunkSize * targetChunkTime / (sw.ElapsedMilliseconds + 1);
                    if (chunkSize < minChunkSize) { chunkSize = minChunkSize; }
                    if (chunkSize > maxChunkSize) { chunkSize = maxChunkSize; }

                    Console.WriteLine($"bytes: {bytesFromDownload.Length} - elapsed: {sw.ElapsedMilliseconds} - chunkSize: {chunkSize}");
                }

                Console.WriteLine($"total downloaded bytes: {downloadedBytes}");
            }

        }

    }

}
