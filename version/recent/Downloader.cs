using System.Collections.Concurrent;

namespace FileDownloader.version.recent
{
    
    public class Downloader
    {
        // HttpClient é seguro para uso concorrente e deve ser reaproveitado
        // durante todo o ciclo de vida da aplicação (evita esgotamento de sockets).
        private static readonly HttpClient httpClient = new HttpClient();

        // ConcurrentBag é thread-safe para adições vindas de múltiplas tasks
        // executando em paralelo (ordem não é garantida, o que é aceitável aqui).
        private static readonly ConcurrentBag<string> cache = new ConcurrentBag<string>();

        public static async Task Main(string[] args)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                string url = "https://example.com/data/" + i;
                tasks.Add(DownloadAsync(url));
            }

            // Aguarda todos os downloads terminarem antes de reportar o resultado.
            await Task.WhenAll(tasks);

            Console.WriteLine("Downloads finished");
            Console.WriteLine("Cache size: " + cache.Count);
        }

        private static async Task DownloadAsync(string url)
        {
            try
            {
                var data = await httpClient.GetStringAsync(url);
                cache.Add(data);
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Falha ao baixar '{url}': {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                Console.Error.WriteLine($"Timeout ao baixar '{url}': {ex.Message}");
            }
        }
    }
}
