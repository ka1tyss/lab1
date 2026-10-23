using System;
using System.Collections.Generic;

namespace IndependentWork6
{
   
    
    // Визначення інтерфейсу контракту сповіщень
    public interface INotifier
    {
        void Send(string message);
    }

    // Реалізація 1: Email сповіщення
    public class EmailNotifier : INotifier
    {
        public void Send(string message)
        {
            Console.WriteLine($"[Email] Відправка електронного листа: \"{message}\"");
        }
    }

    // Реалізація 2: SMS сповіщення
    public class SmsNotifier : INotifier
    {
        public void Send(string message)
        {
            Console.WriteLine($"[SMS] Відправка текстового повідомлення: \"{message}\"");
        }
    }

    // Реалізація 3: Push сповіщення
    public class PushNotifier : INotifier
    {
        public void Send(string message)
        {
            Console.WriteLine($"[Push] Надсилання сповіщення на пристрій: \"{message}\"");
        }
    }


    
    // Абстрактний клас із шаблоном проектування (Template Method)
    public abstract class FileProcessor
    {
        // Загальна логіка відкриття файлу (реалізована в базовому класі)
        protected void OpenFile(string filePath)
        {
            Console.WriteLine($"[Файл] Відкриття файлу за шляхом: {filePath}");
        }

        // Загальна логіка закриття файлу (реалізована в базовому класі)
        protected void CloseFile()
        {
            Console.WriteLine("[Файл] Закриття файлу та звільнення ресурсів.\n");
        }

        // Абстрактний метод, який похідні класи зобов'язані реалізувати самостійно (унікальна логіка)
        public abstract void ProcessContent(string filePath);

        // Шаблонний метод, що координує весь процес обробки
        public void Process(string filePath)
        {
            OpenFile(filePath);
            ProcessContent(filePath);
            CloseFile();
        }
    }

    // Похідний клас 1: Обробник текстових файлів (.txt)
    public class TextFileProcessor : FileProcessor
    {
        public override void ProcessContent(string filePath)
        {
            Console.WriteLine($"[TextProcessor] Читання та аналіз текстового рядка з файлу {filePath}...");
        }
    }

    // Похідний клас 2: Обробник XML файлів (.xml)
    public class XmlFileProcessor : FileProcessor
    {
        public override void ProcessContent(string filePath)
        {
            Console.WriteLine($"[XmlProcessor] Парсинг XML-тегів та валідація структури документа {filePath}...");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            // Налаштування кодування для коректного виведення українських символів
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("==================================================");
            Console.WriteLine(" ДЕМОНСТРАЦІЯ СЦЕНАРІЮ 1: СИСТЕМА СПОВІЩЕНЬ");
            Console.WriteLine("==================================================");
            
            // Створення колекції об'єктів типу INotifier
            List<INotifier> notifiers = new List<INotifier>
            {
                new EmailNotifier(),
                new SmsNotifier(),
                new PushNotifier()
            };

            // Поліморфний виклик методу Send для кожного каналу
            foreach (var notifier in notifiers)
            {
                notifier.Send("Важливе оновлення системи безпеки!");
            }

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ДЕМОНСТРАЦІЯ СЦЕНАРІЮ 2: ОБРОБКА ФАЙЛІВ");
            Console.WriteLine("==================================================");

            // Створення колекції об'єктів базового абстрактного класу FileProcessor
            List<FileProcessor> fileProcessors = new List<FileProcessor>
            {
                new TextFileProcessor(),
                new XmlFileProcessor()
            };

            foreach (var processor in fileProcessors)
            {
                string dummyPath = processor is TextFileProcessor ? "document.txt" : "data.xml";
                processor.Process(dummyPath);
            }

            Console.WriteLine("==================================================");
            Console.WriteLine(" РОБОТУ ПРОГРАМИ ЗАВЕРШЕНО");
            Console.WriteLine("==================================================");
        }
    }
}