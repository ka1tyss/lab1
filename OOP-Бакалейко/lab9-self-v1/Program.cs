using System;
using System.Collections.Generic;

namespace lab9_self_v1
{
    // Допоміжний клас для демонстрації
    public class Product
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    // Статичний клас із власними утилітами для колекцій
    public static class CollectionUtils
    {
        // 1. Метод Map (аналог LINQ Select)
        public static IEnumerable<TResult> Map<TSource, TResult>(
            IEnumerable<TSource> collection,
            Func<TSource, TResult> mapper)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            var result = new List<TResult>();
            foreach (var item in collection)
            {
                result.Add(mapper(item));
            }
            return result;
        }

        // 2. Метод MaxBy (знаходження елемента за критерієм)
        public static T MaxBy<T, TKey>(
            IEnumerable<T> collection,
            Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            T maxItem = default!;
            TKey maxKey = default!;
            bool first = true;

            foreach (var item in collection)
            {
                TKey key = keySelector(item);
                if (first || key.CompareTo(maxKey) > 0)
                {
                    maxKey = key;
                    maxItem = item;
                    first = false;
                }
            }

            if (first)
            {
                throw new InvalidOperationException("Колекція є порожньою.");
            }

            return maxItem;
        }

        // 3. Метод Filter (аналог LINQ Where)
        public static IEnumerable<T> Filter<T>(
            IEnumerable<T> collection,
            Predicate<T> predicate)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var result = new List<T>();
            foreach (var item in collection)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        // 4. Метод ForEach (виконання дії для кожного елемента)
        public static void ForEach<T>(
            IEnumerable<T> collection,
            Action<T> action)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in collection)
            {
                action(item);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Налаштування кодування для коректного відображення української мови
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("==================================================");
            Console.WriteLine(" ДЕМОНСТРАЦІЯ РОБОТИ ВЛАСНИХ УТИЛІТ COLLECTIONUTILS");
            Console.WriteLine("==================================================");

            // Колекція 1: Список чисел (List<int>)
            var numbers = new List<int> { 5, 12, 3, 22, 8, 15 };
            Console.WriteLine("\n--- Колекція чисел ---");
            Console.WriteLine($"Початкові числа: {string.Join(", ", numbers)}");

            // Фільтрація парних чисел
            var evens = CollectionUtils.Filter(numbers, n => n % 2 == 0);
            Console.WriteLine($"Парні числа: {string.Join(", ", evens)}");

            // Піднесення до квадрату (Map)
            var squares = CollectionUtils.Map(evens, n => n * n);
            Console.WriteLine($"Квадрати парних чисел: {string.Join(", ", squares)}");


            // Колекція 2: Список рядків (List<string>)
            var words = new List<string> { "apple", "banana", "pineapple", "kiwi", "peach" };
            Console.WriteLine("\n--- Колекція рядків ---");
            Console.WriteLine($"Слова: {string.Join(", ", words)}");

            // Знаходження найдовшого слова за допомогою MaxBy
            var longestWord = CollectionUtils.MaxBy(words, w => w.Length);
            Console.WriteLine($"Найдовше слово: {longestWord}");


            // Колекція 3: Список продуктів (List<Product>)
            var products = new List<Product>
            {
                new Product { Name = "Laptop", Price = 25000 },
                new Product { Name = "Mouse", Price = 500 },
                new Product { Name = "Keyboard", Price = 1500 },
                new Product { Name = "Monitor", Price = 7000 }
            };

            Console.WriteLine("\n--- Колекція продуктів ---");

            // 1. Map: Отримати список імен
            var names = CollectionUtils.Map(products, p => p.Name);
            Console.WriteLine("Імена продуктів: " + string.Join(", ", names));

            // 2. MaxBy: Знайти найдорожчий продукт
            var mostExpensive = CollectionUtils.MaxBy(products, p => p.Price);
            Console.WriteLine($"Найдорожчий продукт: {mostExpensive.Name} - {mostExpensive.Price} грн");

            // 3. Filter: Знайти продукти дорожчі за 1000
            var expensive = CollectionUtils.Filter(products, p => p.Price > 1000);
            Console.WriteLine("Продукти дорожчі за 1000 грн:");
            CollectionUtils.ForEach(expensive, p => Console.WriteLine($" - {p.Name} ({p.Price} грн)"));

            // 4. ForEach: Вивести всі продукти
            Console.WriteLine("\nПовний прайс-лист (через ForEach):");
            CollectionUtils.ForEach(products, p => Console.WriteLine($" * {p.Name}: {p.Price} грн"));

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" РОБОТУ ПРОГРАМИ ЗАВЕРШЕНО");
            Console.WriteLine("==================================================");
        }
    }
}