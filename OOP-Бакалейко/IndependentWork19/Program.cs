using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

namespace IndependentWork19
{
    // =========================================================================
    // ЧАСТИНА 1: Власний клас винятку (Банківська предметна область)
    // =========================================================================
    public class InsufficientFundsException : Exception
    {
        public decimal RequestedAmount { get; }
        public decimal CurrentBalance { get; }

        public InsufficientFundsException()
            : base("Недостатньо коштів на рахунку для виконання операції.")
        {
        }

        public InsufficientFundsException(string message)
            : base(message)
        {
        }

        public InsufficientFundsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InsufficientFundsException(decimal requestedAmount, decimal currentBalance)
            : base($"Недостатньо коштів: запитано {requestedAmount:C2}, доступно {currentBalance:C2}.")
        {
            RequestedAmount = requestedAmount;
            CurrentBalance = currentBalance;
        }
    }

    public class BankAccount
    {
        public decimal Balance { get; private set; }

        public BankAccount(decimal initialBalance) => Balance = initialBalance;

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума зняття має бути більшою за нуль.", nameof(amount));

            if (amount > Balance)
                throw new InsufficientFundsException(amount, Balance);

            Balance -= amount;
            Console.WriteLine($"[Bank] Успішно знято {amount:C2}. Залишок: {Balance:C2}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==================================================");
            Console.WriteLine(" ЧАСТИНА 1: ДЕМОНСТРАЦІЯ ВЛАСНОГО ВИНЯТКУ");
            Console.WriteLine("==================================================");
            DemonstrateCustomException();

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ЧАСТИНА 2: СЦЕНАРІЙ 1 — RETRY + TIMEOUT (Зовнішнє API)");
            Console.WriteLine("==================================================");
            await RunScenario1_RetryWithTimeoutAsync();

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ЧАСТИНА 3: СЦЕНАРІЙ 2 — CIRCUIT BREAKER (База даних)");
            Console.WriteLine("==================================================");
            await RunScenario2_CircuitBreakerAsync();

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ЧАСТИНА 4: СЦЕНАРІЙ 3 — FALLBACK + RETRY (Черга повідомлень)");
            Console.WriteLine("==================================================");
            await RunScenario3_FallbackAsync();

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" РОБОТУ ПРОГРАМИ ЗАВЕРШЕНО");
            Console.WriteLine("==================================================");
        }

        static void DemonstrateCustomException()
        {
            var account = new BankAccount(1500m);

            try
            {
                account.Withdraw(500m);
                account.Withdraw(2000m);
            }
            catch (InsufficientFundsException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Помилка транзакції] {ex.Message}");
                Console.WriteLine($"   Деталі -> Запитано: {ex.RequestedAmount:C2} | Баланс: {ex.CurrentBalance:C2}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Непередбачена помилка] {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[Bank] Завершення перевірки операції (блок finally).");
            }
        }

        static async Task RunScenario1_RetryWithTimeoutAsync()
        {
            int attemptCounter = 0;

            ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
                .AddRetry(new RetryStrategyOptions<string>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(400),
                    BackoffType = DelayBackoffType.Exponential,
                    ShouldHandle = new PredicateBuilder<string>()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutRejectedException>(),
                    OnRetry = args =>
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[Retry Log] Спроба #{args.AttemptNumber + 1} не вдалася ({args.Outcome.Exception?.GetType().Name}). Очікування {args.RetryDelay.TotalMilliseconds} мс...");
                        Console.ResetColor();
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromMilliseconds(500),
                    OnTimeout = args =>
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"[Timeout Log] Перевищено ліміт часу ({args.Timeout.TotalMilliseconds} мс) на виконання запиту!");
                        Console.ResetColor();
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            try
            {
                string response = await pipeline.ExecuteAsync(async token =>
                {
                    attemptCounter++;
                    Console.WriteLine($"[API Client] Виконання запиту (спроба #{attemptCounter})...");

                    if (attemptCounter == 1)
                    {
                        throw new HttpRequestException("503 Service Unavailable");
                    }
                    if (attemptCounter == 2)
                    {
                        await Task.Delay(800, token);
                    }

                    await Task.Delay(100, token);
                    return "{\"status\": \"OK\", \"txId\": \"99812\"}";
                }, CancellationToken.None);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[API Client] Успішна відповідь: {response}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API Client] Усі спроби вичерпано: {ex.Message}");
            }
        }

        static async Task RunScenario2_CircuitBreakerAsync()
        {
            ResiliencePipeline dbPipeline = new ResiliencePipelineBuilder()
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(2),
                    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
                    OnOpened = args =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[CircuitBreaker] Ланцюг РОЗІМКНУТО (OPEN) на {args.BreakDuration.TotalSeconds} с через критичну кількість збоїв БД!");
                        Console.ResetColor();
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[CircuitBreaker] Ланцюг ЗАМКНУТО (CLOSED). Роботу БД відновлено.");
                        Console.ResetColor();
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = args =>
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("[CircuitBreaker] Ланцюг у стані HALF-OPEN. Пробний запит до БД...");
                        Console.ResetColor();
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            for (int i = 1; i <= 6; i++)
            {
                try
                {
                    if (i == 5)
                    {
                        Console.WriteLine("[System] Очікування 2.2 секунди для відновлення вікна Circuit Breaker...");
                        await Task.Delay(2200);
                    }

                    await dbPipeline.ExecuteAsync(token =>
                    {
                        Console.WriteLine($"[DB Query #{i}] Спроба виконання SQL-запиту...");
                        if (i <= 3)
                        {
                            throw new InvalidOperationException("Timeout expired. Connection pool exhausted.");
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[DB Query #{i}] Запит успішно виконано!");
                        Console.ResetColor();
                        return ValueTask.CompletedTask;
                    }, CancellationToken.None);
                }
                catch (BrokenCircuitException)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[DB Query #{i}] Заблоковано миттєво (BrokenCircuitException) без навантаження на БД!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB Query #{i}] Помилка БД: {ex.Message}");
                }
            }
        }

        static async Task RunScenario3_FallbackAsync()
        {
            ResiliencePipeline<string> queuePipeline = new ResiliencePipelineBuilder<string>()
                .AddFallback(new FallbackStrategyOptions<string>
                {
                    ShouldHandle = new PredicateBuilder<string>().Handle<Exception>(),
                    FallbackAction = args =>
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[Fallback] Черга перевантажена. Зберігаємо повідомлення у локальний Outbox-файл.");
                        Console.ResetColor();
                        return Outcome.FromResultAsValueTask("SAVED_TO_LOCAL_OUTBOX");
                    }
                })
                .AddRetry(new RetryStrategyOptions<string>
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromMilliseconds(200),
                    OnRetry = args =>
                    {
                        Console.WriteLine($"[Queue Retry] Повторна спроба відправки #{args.AttemptNumber + 1}...");
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            string status = await queuePipeline.ExecuteAsync(async token =>
            {
                Console.WriteLine("[Broker] Спроба відправки події в RabbitMQ...");
                await Task.Delay(50, token);
                throw new Exception("BrokerQueueFullException: Черга переповнена.");
#pragma warning disable CS0162
                return "SENT";
#pragma warning restore CS0162
            }, CancellationToken.None);

            Console.WriteLine($"[Broker] Підсумковий статус доставки: {status}");
        }
    }
}