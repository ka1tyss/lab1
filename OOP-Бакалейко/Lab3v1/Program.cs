using System;

namespace Lab3
{
    public class FileLogger : IDisposable
    {
        private bool _disposed = false;
        private string _filePath;
        private bool _isFileOpen;

        public string FilePath => _filePath;
        public bool IsFileOpen => _isFileOpen;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
            _isFileOpen = true;
            Console.WriteLine($"[FileLogger] Файл '{_filePath}' відкрито (ресурс виділено).");
        }

        public void Log(string message)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileLogger), "Неможливо виконати Log(): об'єкт уже знищено!");
            }

            if (_isFileOpen)
            {
                Console.WriteLine($"[LOG to {_filePath}]: {message}");
            }
            else
            {
                Console.WriteLine($"[FileLogger] Помилка: файл '{_filePath}' закрито.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.WriteLine($"[Dispose(true)] Звільнення керованих ресурсів для '{_filePath}'.");
                }

                if (_isFileOpen)
                {
                    Console.WriteLine($"[Dispose] Файл '{_filePath}' закрито (некерований ресурс звільнено).");
                    _isFileOpen = false;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileLogger()
        {
            Console.WriteLine($"[~FileLogger] Фіналізатор викликано для '{_filePath}'!");
            Dispose(false);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== ДЕМОНСТРАЦІЯ ЖИТТЄВОГО ЦИКЛУ ОБ'ЄКТА (FileLogger) ===\n");

            Console.WriteLine("--- Сценарій 1: Автоматичне звільнення через using ---");
            using (var logger1 = new FileLogger("app_using.log"))
            {
                logger1.Log("Перше повідомлення");
                logger1.Log("Друге повідомлення");
            }
            Console.WriteLine();

            Console.WriteLine("--- Сценарій 2: Явний виклик Dispose() ---");
            var logger2 = new FileLogger("app_explicit.log");
            logger2.Log("Запис перед закриттям");
            logger2.Dispose();
            Console.WriteLine();

            Console.WriteLine("--- Сценарій 3: Демонстрація роботи деструктора через GC ---");
            CreateAndForgetLogger();

            Console.WriteLine("Запуск збирача сміття (GC.Collect)...");
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("\nПрограму завершено.");
        }

        static void CreateAndForgetLogger()
        {
            var logger3 = new FileLogger("app_gc.log");
            logger3.Log("Запис у файл без виклику Dispose()");
        }
    }
}