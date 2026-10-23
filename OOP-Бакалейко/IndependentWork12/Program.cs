using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace IndependentWork12
{
    class Program
    {
        // Обчислювально інтенсивна операція: перевірка на просте число з додатковим навантаженням
        static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            int boundary = (int)Math.Floor(Math.Sqrt(number));
            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }

            // Додаткове штучне навантаження (математичні обчислення), 
            // щоб різниця між LINQ та PLINQ була наочнішою на швидких CPU
            double dummy = 0;
            for (int i = 0; i < 50; i++)
            {
                dummy += Math.Sin(number) * Math.Cos(i);
            }

            return true;
        }

        static void Main(string[] args)
        {
            // Налаштування кодування для коректного виведення українських символів
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==================================================");
            Console.WriteLine(" ЧАСТИНА 1: ДОСЛІДЖЕННЯ ПРОДУКТИВНОСТІ LINQ vs PLINQ");
            Console.WriteLine("==================================================");

            // Тестування на різних обсягах даних: 1 млн, 5 млн, 10 млн елементів
            int[] sizes = { 1_000_000, 5_000_000, 10_000_000 };

            foreach (var size in sizes)
            {
                Console.WriteLine($"\n--- Генерація колекції з {size:N0} випадкових чисел ---");
                var rand = new Random(42);
                var data = Enumerable.Range(0, size).Select(_ => rand.Next(1, 10_000_000)).ToList();

                var sw = new Stopwatch();

                // 1. Послідовний LINQ
                sw.Start();
                var seqResult = data.Where(n => IsPrime(n)).ToList();
                sw.Stop();
                long seqTicks = sw.ElapsedTicks;
                long seqMs = sw.ElapsedMilliseconds;
                Console.WriteLine($"[LINQ] Час виконання: {seqMs} ms ({seqTicks} ticks) | Знайдено простих: {seqResult.Count}");

                // 2. Паралельний PLINQ (.AsParallel())
                sw.Restart();
                var parallelResult = data.AsParallel().Where(n => IsPrime(n)).ToList();
                sw.Stop();
                long parTicks = sw.ElapsedTicks;
                long parMs = sw.ElapsedMilliseconds;
                Console.WriteLine($"[PLINQ] Час виконання: {parMs} ms ({parTicks} ticks) | Знайдено простих: {parallelResult.Count}");

                // Розрахунок прискорення (Speedup)
                if (parMs > 0)
                {
                    double speedup = (double)seqMs / parMs;
                    Console.WriteLine($"-> Прискорення PLINQ: {speedup:F2}x");
                }
            }


            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ЧАСТИНА 2: ДОСЛІДЖЕННЯ ПРОБЛЕМ БЕЗПЕКИ (ПОБІЧНІ ЕФЕКТИ)");
            Console.WriteLine("==================================================");

            // Підготовка даних для демонстрації проблеми
            var sourceData = Enumerable.Range(1, 100_000).ToList();

            // Сценарій А: Небезпечне модифікування спільної змінної (Race Condition)
            Console.WriteLine("\n1. Небезпечний запуск PLINQ зі зміною спільного лічильника без синхронізації:");
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                int unsafeCounter = 0;
                // Паралельна обробка модифікує зовнішню змінну без блокування
                sourceData.AsParallel().ForAll(n =>
                {
                    if (n % 2 == 0)
                    {
                        unsafeCounter++; // ПОПЕРЕДЖЕННЯ: Гонка потоків (Race Condition)!
                    }
                });

                // Очікуваний результат для 100_000 елементів (де діапазон 1..100k містить рівно 50_000 парних чисел)
                Console.WriteLine($" Спроба {attempt}: Очікувалось = 50000, Отримано = {unsafeCounter} (через втрату даних)");
            }


            // Сценарій Б: Безпечний запуск за допомогою Interlocked
            Console.WriteLine("\n2. Безпечний запуск PLINQ з використанням Interlocked.Increment:");
            {
                int safeCounterInterlocked = 0;
                sourceData.AsParallel().ForAll(n =>
                {
                    if (n % 2 == 0)
                    {
                        Interlocked.Increment(ref safeCounterInterlocked); // Потокобезпечно
                    }
                });
                Console.WriteLine($" Результат з Interlocked: {safeCounterInterlocked} (Очікується 50000)");
            }


            // Сценарій В: Безпечний запуск за допомогою блокування (lock)
            Console.WriteLine("\n3. Безпечний запуск PLINQ з використанням lock:");
            {
                int safeCounterLock = 0;
                object syncLock = new object();

                sourceData.AsParallel().ForAll(n =>
                {
                    if (n % 2 == 0)
                    {
                        lock (syncLock)
                        {
                            safeCounterLock++;
                        }
                    }
                });
                Console.WriteLine($" Результат з lock: {safeCounterLock} (Очікується 50000)");
            }


            // Сценарій Г: Ідіоматичний підхід без побічних ефектів (aggregation / Where + Count)
            Console.WriteLine("\n4. Рекомендований чистий підхід без побічних ефектів (Functional style):");
            {
                int cleanCount = sourceData.AsParallel().Where(n => n % 2 == 0).Count();
                Console.WriteLine($" Результат декларативного підходу: {cleanCount} (Очікується 50000)");
            }

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" РОБОТУ ПРОГРАМИ ЗАВЕРШЕНО");
            Console.WriteLine("==================================================");
        }
    }
}