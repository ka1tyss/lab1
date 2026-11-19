using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace IndependentWork14
{
    public class RetryPolicy
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _initialDelay;
        private readonly double _jitterFactor;
        private readonly Random _random = new Random();

        public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor = 0.2)
        {
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetries), "Кількість спроб не може бути від'ємною.");
            if (initialDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(initialDelay), "Початкова затримка не може бути від'ємною.");
            if (jitterFactor < 0 || jitterFactor >= 1)
                throw new ArgumentOutOfRangeException(nameof(jitterFactor), "Коефіцієнт jitter має бути в межах [0, 1).");

            _maxRetries = maxRetries;
            _initialDelay = initialDelay;
            _jitterFactor = jitterFactor;
        }

        public T Execute<T>(Func<T> operation, Func<Exception, bool>? shouldRetry = null)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            int retryAttempt = 0;

            while (true)
            {
                try
                {
                    Console.WriteLine($"[RetryPolicy] Виконання спроби #{retryAttempt + 1}...");
                    T result = operation();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[RetryPolicy] Операція успішна на спробі #{retryAttempt + 1}.");
                    Console.ResetColor();
                    return result;
                }
                catch (Exception ex)
                {
                    bool canRetry = retryAttempt < _maxRetries && (shouldRetry == null || shouldRetry(ex));

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[RetryPolicy] Помилка на спробі #{retryAttempt + 1} ({ex.GetType().Name}): {ex.Message}");
                    Console.ResetColor();

                    if (!canRetry)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[RetryPolicy] Повторні спроби неможливі або вичерпано (виконано повторів: {retryAttempt}). Прокидаємо виняток.");
                        Console.ResetColor();
                        throw;
                    }

                    double jitterMultiplier = 1.0 + _random.NextDouble() * _jitterFactor * 2.0 - _jitterFactor;
                    double baseDelayMs = _initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt);
                    double finalDelayMs = Math.Max(0, baseDelayMs * jitterMultiplier);
                    TimeSpan delay = TimeSpan.FromMilliseconds(finalDelayMs);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[RetryPolicy] Очікування {delay.TotalMilliseconds:F1} мс перед повтором #{retryAttempt + 2}...");
                    Console.ResetColor();

                    Thread.Sleep(delay);
                    retryAttempt++;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var retryPolicy = new RetryPolicy(
                maxRetries: 3,
                initialDelay: TimeSpan.FromMilliseconds(300),
                jitterFactor: 0.2
            );

            Console.WriteLine("==================================================");
            Console.WriteLine(" СЦЕНАРІЙ 1: ОПЕРАЦІЯ З УСПІШНИМ ЗАВЕРШЕННЯМ");
            Console.WriteLine("==================================================");

            int attemptCounter = 0;
            string result = retryPolicy.Execute(
                operation: () =>
                {
                    attemptCounter++;
                    if (attemptCounter <= 2)
                    {
                        throw new HttpRequestException($"Тимчасовий збій з'єднання (спроба {attemptCounter})");
                    }
                    return "Дані успішно отримано з віддаленого сервісу";
                },
                shouldRetry: ex => ex is HttpRequestException
            );

            Console.WriteLine($"Результат сценарію 1: {result}\n");

            Console.WriteLine("==================================================");
            Console.WriteLine(" СЦЕНАРІЙ 2: ОПЕРАЦІЯ, ЯКА ЗАВЖДИ ЗАВЕРШУЄТЬСЯ НЕВДАЧЕЮ");
            Console.WriteLine("==================================================");

            try
            {
                retryPolicy.Execute<int>(
                    operation: () =>
                    {
                        throw new IOException("Сервіс недоступний: вичерпано ліміт запитів");
                    },
                    shouldRetry: ex => ex is IOException
                );
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Main] Перехоплено фінальний виняток після всіх спроб: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nРоботу завершено.");
        }
    }
}