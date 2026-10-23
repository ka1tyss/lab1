using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IndependentWork5
{
    // ==========================================
    // 1. ДЕМОНСТРАЦІЯ ПОЛІМОРФІЗМУ ЧЕРЕЗ System.Object
    // ==========================================
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

        public override string ToString() => $"Товар: {Name}, Ціна: {Price} грн";
    }

    public class Customer
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        public Customer(string fullName, string email)
        {
            FullName = fullName;
            Email = email;
        }

        public override string ToString() => $"Клієнт: {FullName} ({Email})";
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int Quantity { get; set; }

        public Order(int orderId, int quantity)
        {
            OrderId = orderId;
            Quantity = quantity;
        }

        public override string ToString() => $"Замовлення №{OrderId}, Кількість одиниць: {Quantity}";
    }

    // ==========================================
    // 2. ДЕМОНСТРАЦІЯ ПОЛІМОРФІЗМУ ЧЕРЕЗ IEnumerable<string>
    // ==========================================
    public class MyCustomCollection : IEnumerable<string>
    {
        private List<string> _items = new List<string>();

        public void Add(string item)
        {
            _items.Add(item);
        }

        // Реалізація IEnumerable<string>
        public IEnumerator<string> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
        }

        // Необхідна реалізація для неспорідненого IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class Program
    {
        // ==========================================
        // 3. МЕТОД ДЛЯ ПОЛІМОРФІЗМУ ЧЕРЕЗ Stream
        // ==========================================
        static void ProcessStream(Stream stream)
        {
            Console.WriteLine($"[ProcessStream] Обробка потоку типу: {stream.GetType().Name}");
            Console.WriteLine($"[ProcessStream] Чи доступний для читання: {stream.CanRead}");
            Console.WriteLine($"[ProcessStream] Довжина потоку: {stream.Length} байт");

            // Читаємо перші байти поліморфно, незалежно від того, файл це чи пам'ять
            stream.Position = 0;
            byte[] buffer = new byte[10];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            
            string contentPreview = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"[ProcessStream] Зчитано вміст (перші байти): \"{contentPreview}\"\n");
        }

        static void Main(string[] args)
        {
            // Налаштування кодування для коректного відображення української мови в терміналі
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== 1. ПОЛІМОРФІЗМ ЧЕРЕЗ SYSTEM.OBJECT ===");
            List<object> objects = new List<object>
            {
                new Product("Ноутбук", 32500.00m),
                new Customer("Олександр Петренко", "alex@example.com"),
                new Order(105, 3)
            };

            // Поліморфний виклик методу ToString() через базовий тип object
            foreach (var obj in objects)
            {
                Console.WriteLine(obj.ToString()); // Викликається перезначений ToString() відповідного класу
            }

            Console.WriteLine("\n=== 2. ПОЛІМОРФІЗМ ЧЕРЕЗ IENUMERABLE<T> ===");
            MyCustomCollection customCollection = new MyCustomCollection();
            customCollection.Add("Елемент А");
            customCollection.Add("Елемент Б");
            customCollection.Add("Елемент В");

            // Цикл foreach працює завдяки реалізації IEnumerable<string>
            Console.WriteLine("Перебір власної колекції через цикл foreach:");
            foreach (var item in customCollection)
            {
                Console.WriteLine($"- {item}");
            }

            Console.WriteLine("\n=== 3. ПОЛІМОРФІЗМ ЧЕРЕЗ АБСТРАКТНИЙ КЛАС STREAM ===");
            
            // Створення тимчасового текстового файлу для FileStream
            string tempFilePath = "test_stream.txt";
            File.WriteAllText(tempFilePath, "Це тестові дані для перевірки роботи FileStream у .NET!", Encoding.UTF8);

            // Робота з FileStream
            using (Stream fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
            {
                ProcessStream(fileStream);
            }

            // Робота з MemoryStream
            byte[] memoryData = Encoding.UTF8.GetBytes("Дані, збережені безпосередньо в оперативній пам'яті комп'ютера.");
            using (Stream memoryStream = new MemoryStream(memoryData))
            {
                ProcessStream(memoryStream);
            }

            // Очищення тимчасового файлу
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            Console.WriteLine("=== КІНЕЦЬ ДЕМОНСТРАЦІЇ ===");
        }
    }
}