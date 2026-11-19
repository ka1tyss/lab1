using System;
using System.Text;

namespace lab19v1
{
    // =========================================================================
    // 1. Власні типи винятків
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

    public class InvalidAccountOperationException : Exception
    {
        public InvalidAccountOperationException()
            : base("Некоректна операція з банківським рахунком.")
        {
        }

        public InvalidAccountOperationException(string message)
            : base(message)
        {
        }

        public InvalidAccountOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    // =========================================================================
    // 2. Узагальнений клас Result<T>
    // =========================================================================
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string Error { get; }

        private Result(T? value, bool isSuccess, string error)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(default, false, error);
        }

        public static Result<T> Failure(Exception ex)
        {
            return new Result<T>(default, false, ex?.Message ?? "Невідома помилка");
        }
    }

    // =========================================================================
    // 3. Клас AccountService (Варіант 1)
    // =========================================================================
    public class AccountService
    {
        public decimal Balance { get; private set; }

        public AccountService(decimal initialBalance = 0m)
        {
            if (initialBalance < 0)
                throw new InvalidAccountOperationException("Початковий баланс не може бути від'ємним.");

            Balance = initialBalance;
        }

        // Викидає власний виняток InsufficientFundsException, якщо недостатньо коштів
        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidAccountOperationException("Сума зняття має бути більшою за нуль.");

            if (amount > Balance)
                throw new InsufficientFundsException(amount, Balance);

            Balance -= amount;
        }

        // Повертає Result<decimal> замість винятку для очікуваної помилки валідації (amount <= 0)
        public Result<decimal> Deposit(decimal amount)
        {
            if (amount <= 0)
                return Result<decimal>.Failure($"Сума поповнення має бути більшою за нуль (передано {amount:C2}).");

            Balance += amount;
            return Result<decimal>.Success(Balance);
        }

        // Додатковий метод з Result<T> для перевірки/переказу з очікуваними сценаріями
        public Result<decimal> CheckAvailability(decimal amount)
        {
            if (amount <= 0)
                return Result<decimal>.Failure("Сума перевірки має бути більшою за нуль.");

            if (amount > Balance)
                return Result<decimal>.Failure($"Недостатньо коштів для запиту {amount:C2} (поточний баланс: {Balance:C2}).");

            return Result<decimal>.Success(Balance - amount);
        }
    }

    // =========================================================================
    // 4. Демонстрація в методі Main
    // =========================================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var accountService = new AccountService(1000m);
            Console.WriteLine($"Початковий баланс рахунку: {accountService.Balance:C2}\n");

            Console.WriteLine("==================================================");
            Console.WriteLine(" 1. РОБОТА З RESULT<T> — МЕТОД DEPOSIT");
            Console.WriteLine("==================================================");

            // Валідне поповнення
            var validDeposit = accountService.Deposit(500m);
            PrintResult(validDeposit, "Поповнення на 500,00 ₴");

            // Невалідне поповнення (сума <= 0) — повертає Result.Failure без винятку
            var invalidDeposit = accountService.Deposit(-150m);
            PrintResult(invalidDeposit, "Поповнення на -150,00 ₴");

            var zeroDeposit = accountService.Deposit(0m);
            PrintResult(zeroDeposit, "Поповнення на 0,00 ₴");

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" 2. ПЕРЕВІРКА ДОСТУПНОСТІ ЧЕРЕЗ RESULT<T>");
            Console.WriteLine("==================================================");
            var checkResult = accountService.CheckAvailability(2500m);
            PrintResult(checkResult, "Перевірка зняття 2 500,00 ₴");

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" 3. ВИКЛИК WITHDRAW ТА ОБРОБКА ВИНЯТКІВ (TRY-CATCH)");
            Console.WriteLine("==================================================");

            try
            {
                Console.WriteLine("Спроба зняти 400,00 ₴...");
                accountService.Withdraw(400m);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Успішно знято 400,00 ₴. Новий баланс: {accountService.Balance:C2}");
                Console.ResetColor();

                Console.WriteLine("Спроба зняти 2 000,00 ₴...");
                accountService.Withdraw(2000m);
            }
            catch (InsufficientFundsException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Перехоплено InsufficientFundsException: {ex.Message}");
                Console.WriteLine($"Запитано: {ex.RequestedAmount:C2}, Поточний баланс: {ex.CurrentBalance:C2}");
                Console.ResetColor();
            }
            catch (InvalidAccountOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Перехоплено InvalidAccountOperationException: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine($"Фінальний баланс після спроб зняття: {accountService.Balance:C2}");
            }

            Console.WriteLine("\nРоботу завершено.");
        }

        static void PrintResult(Result<decimal> result, string operationName)
        {
            if (result.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{operationName}] Успіх! Новий/очікуваний баланс: {result.Value:C2}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{operationName}] Невдача (Result.Failure): {result.Error}");
                Console.ResetColor();
            }
        }
    }
}