using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace lab18v1
{
    // =========================================================================
    // 1. Клас FileProcessor (імітація роботи з файлами)
    // =========================================================================
    public class FileProcessor
    {
        private int _readAttempt = 0;

        public string ReadFile(string path)
        {
            _readAttempt++;
            Console.WriteLine($"[FileProcessor] Спроба читання файлу '{path}' (виклик #{_readAttempt})...");

            // Імітуємо FileNotFoundException перші 2 рази, потім успіх
            if (_readAttempt <= 2)
            {
                throw new FileNotFoundException($"Файл '{path}' ще не з'явився або тимчасово недоступний.", path);
            }

            return $"Вміст файлу '{path}': [Конфігурація завантажена успішно]";
        }
    }

    // =========================================================================
    // 2. Клас NetworkClient (імітація мережевих запитів)
    // =========================================================================
    public class NetworkClient
    {
        private int _downloadAttempt = 0;

        public string DownloadData(string url)
        {
            _downloadAttempt++;
            Console.WriteLine($"[NetworkClient] Спроба завантаження з '{url}' (виклик #{_downloadAttempt})...");

            // Імітуємо HttpRequestException перші 3 рази, потім успіх
            if (_downloadAttempt <= 3)
            {
                throw new HttpRequestException($"Тимчасова мережева помилка при зверненні до '{url}' (спроба {_downloadAttempt}).");
            }

            return $"Дані з '{url}': {{\"status\": \"ok\", \"items\": [1, 2, 3]}}";
        }
    }

    // =========================================================================
    // 3. Узагальнений допоміжний клас RetryHelper
    // =========================================================================
    public static class RetryHelper
    {
        public static T ExecuteWithRetry<T>(
            Func<T> operation,
            int retryCount = 3,
            TimeSpan initialDelay = default,
            Func<Exception, bool>? shouldRetry = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (retryCount < 0)
                throw new ArgumentOutOfRangeException(nameof(retryCount), "Кількість повторів не може бути від'ємною.");

            if (initialDelay == default)
                initialDelay = TimeSpan.FromMilliseconds(200);

            int retryAttempt = 0;

            try
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine($"[RetryHelper] Виконання спроби #{retryAttempt + 1}...");
                        T result = operation();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[RetryHelper] Успішно виконано на спробі #{retryAttempt + 1}.");
                        Console.ResetColor();

                        return result;
                    }
                    catch (Exception ex)
                    {
                        bool isRetryable = shouldRetry == null ? true : shouldRetry(ex);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[RetryHelper] Помилка на спробі #{retryAttempt + 1} ({ex.GetType().Name}): {ex.Message}");
                        Console.ResetColor();

                        if (!isRetryable)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[RetryHelper] Виняток {ex.GetType().Name} не підлягає повтору згідно з shouldRetry. Перериваємо.");
                            Console.ResetColor();
                            throw;
                        }

                        if (retryAttempt >= retryCount)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[RetryHelper] Вичерпано ліміт повторних спроб ({retryCount}). Прокидаємо виняток.");
                            Console.ResetColor();
                            throw;
                        }

                        // Експоненційна затримка: initialDelay * 2^retryAttempt
                        double multiplier = Math.Pow(2, retryAttempt);
                        TimeSpan delay = TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * multiplier);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[RetryHelper] Затримка {delay.TotalMilliseconds:F0} мс перед спробою #{retryAttempt + 2}...");
                        Console.ResetColor();

                        Thread.Sleep(delay);
                        retryAttempt++;
                    }
                }
            }
            finally
            {
                Console.WriteLine($"[RetryHelper] Завершення роботи ExecuteWithRetry (усього повторів виконано: {retryAttempt}).");
            }
        }
    }

    // =========================================================================
    // 4. Демонстрація в методі Main (Варіант 1)
    // =========================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var fileProcessor = new FileProcessor();
            var networkClient = new NetworkClient();

            // Вибіркова умова повтору для Варіанту 1:
            // Повторювати для FileNotFoundException та HttpRequestException
            Func<Exception, bool> shouldRetry = ex =>
                ex is FileNotFoundException || ex is HttpRequestException;

            Console.WriteLine("==================================================");
            Console.WriteLine(" СЦЕНАРІЙ 1: ЧИТАННЯ ФАЙЛУ (FileProcessor)");
            Console.WriteLine("==================================================");
            try
            {
                string fileContent = RetryHelper.ExecuteWithRetry(
                    operation: () => fileProcessor.ReadFile("config.json"),
                    retryCount: 3,
                    initialDelay: TimeSpan.FromMilliseconds(250),
                    shouldRetry: shouldRetry
                );
                Console.WriteLine($"[Результат] {fileContent}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Помилка] Не вдалося прочитати файл: {ex.Message}\n");
            }

            Console.WriteLine("==================================================");
            Console.WriteLine(" СЦЕНАРІЙ 2: ЗАВАНТАЖЕННЯ ДАНИХ (NetworkClient)");
            Console.WriteLine("==================================================");
            try
            {
                string downloadedData = RetryHelper.ExecuteWithRetry(
                    operation: () => networkClient.DownloadData("https://api.example.com/data"),
                    retryCount: 4,
                    initialDelay: TimeSpan.FromMilliseconds(200),
                    shouldRetry: shouldRetry
                );
                Console.WriteLine($"[Результат] {downloadedData}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Помилка] Не вдалося завантажити дані: {ex.Message}\n");
            }

            Console.WriteLine("==================================================");
            Console.WriteLine(" СЦЕНАРІЙ 3: ВИБІРКОВА ОБРОБКА (shouldRetry)");
            Console.WriteLine("==================================================");
            try
            {
                // UnauthorizedAccessException не входить у shouldRetry — має впасти одразу без повторів
                RetryHelper.ExecuteWithRetry<string>(
                    operation: () =>
                    {
                        Console.WriteLine("[App] Спроба доступу до захищеного ресурсу...");
                        throw new UnauthorizedAccessException("Відмовлено в доступі до ресурсу.");
                    },
                    retryCount: 3,
                    initialDelay: TimeSpan.FromMilliseconds(200),
                    shouldRetry: shouldRetry
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Результат] Очікувано зупинено без зайвих повторів: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nРоботу завершено.");
        }
    }
}