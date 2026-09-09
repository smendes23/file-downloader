using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileDownloader.version.old
{

    public class Downloader
    {
        // BUG 1: List<string> não é thread-safe para adições concorrentes vindas de múltiplas tasks.
        private static List<string> cache = new List<string>();
        public static async Task Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                // BUG 2: chamada sem await/sem guardar a Task (fire-and-forget) — Main segue sem esperar os downloads.
                // E as exceções desta task ficam não observadas (unobserved task exception).
                DownloadAsync("https://example.com/data/" + i);
            }
            Console.WriteLine("Downloads started");
            // BUG 3: cache ainda pode estar vazio aqui, pois os downloads não terminaram.
            Console.WriteLine("Cache size: " + cache.Count);
        }
        private static async Task DownloadAsync(string url)
        {
            // BUG 4: novo HttpClient criado e descartado a cada chamada — risco de esgotamento de sockets.
            using (HttpClient client = new HttpClient())
            {
                // BUG 5: nenhum tratamento de erro — falha aqui derruba o download silenciosamente.
                // E nenhum timeout/CancellationToken configurado (usa o padrão de 100s do HttpClient).
                var data = await client.GetStringAsync(url);
                cache.Add(data);
            }
        }
    }
}
