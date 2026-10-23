using System;
using System.Collections.Generic;

namespace IndependentWork7
{
    // Узагальнений клас кешу з політикою FIFO (First In, First Out)
    public class Cache<TKey, TValue> where TKey : notnull
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, TValue> _cache;
        private readonly Queue<TKey> _keysOrder;

        public Cache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Ємність кешу повинна бути більшою за нуль.", nameof(capacity));
            }

            _capacity = capacity;
            _cache = new Dictionary<TKey, TValue>(capacity);
            _keysOrder = new Queue<TKey>(capacity);
        }

        public int Count => _cache.Count;

        public void Add(TKey key, TValue value)
        {
            if (_cache.ContainsKey(key))
            {
                // Якщо ключ уже існує, оновлюємо значення
                _cache[key] = value;
                return;
            }

            // Якщо досягнуто максимальної ємності, видаляємо найстаріший елемент (FIFO)
            if (_cache.Count >= _capacity)
            {
                if (_keysOrder.Count > 0)
                {
                    TKey oldestKey = _keysOrder.Dequeue();
                    _cache.Remove(oldestKey);
                }
            }

            // Додаємо новий елемент
            _cache[key] = value;
            _keysOrder.Enqueue(key);
        }

        public TValue Get(TKey key)
        {
            if (_cache.TryGetValue(key, out TValue? value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Ключ '{key}' не знайдено в кеші.");
        }

        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== ДЕМОНСТРАЦІЯ РОБОТИ FIFO CACHE ===\n");

            // Створюємо кеш ємністю 3 елементи
            Cache<int, string> cache = new Cache<int, string>(3);

            Console.WriteLine("Додаємо 3 елементи (1: 'One', 2: 'Two', 3: 'Three'):");
            cache.Add(1, "One");
            cache.Add(2, "Two");
            cache.Add(3, "Three");

            Console.WriteLine($"Поточна кількість елементів у кеші (Count): {cache.Count}");
            Console.WriteLine($"Чи містить ключ 1? {cache.ContainsKey(1)}");
            Console.WriteLine($"Отримуємо значення за ключем 2: {cache.Get(2)}");

            Console.WriteLine("\nДодаємо 4-й елемент (4: 'Four'), що перевищує ємність (має видалитися найстаріший ключ — 1):");
            cache.Add(4, "Four");

            Console.WriteLine($"Чи містить ключ 1 після переповнення? {cache.ContainsKey(1)} (очікується false)");
            Console.WriteLine($"Чи містить ключ 4? {cache.ContainsKey(4)} (очікується true)");
            Console.WriteLine($"Поточна кількість елементів (Count): {cache.Count}");

            try
            {
                Console.WriteLine("\nСпроба отримати видалений елемент за ключем 1:");
                cache.Get(1);
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"Зловлено виняток: {ex.Message}");
            }

            Console.WriteLine("\n=== КІНЕЦЬ ДЕМОНСТРАЦІЇ ===");
        }
    }
}