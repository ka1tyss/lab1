using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IndependentWork11
{
    // ==========================================
    // СУТНОСТІ ДАНИХ (Entities)
    // ==========================================
    public class Sale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime SaleDate { get; set; }
        public int CustomerId { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==================================================");
            Console.WriteLine(" ГЕНЕРАЦІЯ ТЕСТОВИХ ДАНИХ (ETL EXTRACT)");
            Console.WriteLine("==================================================");

            // Генеруємо тестові дані з фіксованим Seed для відтворюваності
            var rand = new Random(42);

            var categories = new[] { "Electronics", "Clothing", "Home & Kitchen", "Books", "Sports" };
            var cities = new[] { "Kyiv", "Lviv", "Kharkiv", "Dnipro", "Odesa" };

            // 1. Генерація продуктів (20 штук)
            var products = Enumerable.Range(1, 20).Select(i => new Product
            {
                Id = i,
                Name = $"Product {i}",
                Category = categories[rand.Next(categories.Length)],
                UnitPrice = rand.Next(100, 5000)
            }).ToList();

            // 2. Генерація клієнтів (50 штук)
            var customers = Enumerable.Range(1, 50).Select(i => new Customer
            {
                Id = i,
                Name = $"Customer {i}",
                City = cities[rand.Next(cities.Length)]
            }).ToList();

            // 3. Генерація продажу (500 записів)
            var sales = Enumerable.Range(1, 500).Select(i => new Sale
            {
                Id = i,
                ProductId = rand.Next(1, 21),
                Quantity = rand.Next(1, 10),
                SaleDate = new DateTime(2025, rand.Next(1, 13), rand.Next(1, 28)),
                CustomerId = rand.Next(1, 51)
            }).ToList();

            Console.WriteLine($"Згенеровано: Продуктів: {products.Count}, Клієнтів: {customers.Count}, Продажів: {sales.Count}\n");

            Console.WriteLine("==================================================");
            Console.WriteLine(" ЗАПУСК LINQ-ПАЙПЛАЙНІВ ТА АНАЛІТИКА");
            Console.WriteLine("==================================================");

            // Виклик пайплайнів
            RunPipeline1_TopProductsByCategory(sales, products);
            RunPipeline2_RevenueByCustomerCity(sales, products, customers);
            RunPipeline3_SalesByYearAndMonth(sales, products);
            RunPipeline4_TopActiveCustomers(sales, customers);

            Console.WriteLine("==================================================");
            Console.WriteLine(" РОБОТУ ПРОГРАМИ ЗАВЕРШЕНО");
            Console.WriteLine("==================================================");
        }

        // ==========================================
        // ПАЙПЛАЙН 1: Найпопулярніші продукти за категоріями
        // ==========================================
        public static void RunPipeline1_TopProductsByCategory(List<Sale> sales, List<Product> products)
        {
            var sw = Stopwatch.StartNew();

            // ETL Pipeline: Згрупувати продажі -> Об'єднати з продуктами -> Згрупувати за категоріями -> Обрати топ-3
            var result = sales
                .GroupBy(s => s.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSoldQuantity = g.Sum(s => s.Quantity)
                })
                .Join(products,
                    s => s.ProductId,
                    p => p.Id,
                    (s, p) => new { p.Name, p.Category, s.TotalSoldQuantity })
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TopProducts = g.OrderByDescending(x => x.TotalSoldQuantity)
                                   .Take(3)
                                   .ToList()
                })
                .ToList();

            sw.Stop();

            Console.WriteLine($"\n[Пайплайн 1] Найпопулярніші продукти за категоріями (Час: {sw.ElapsedTicks} ticks / {sw.ElapsedMilliseconds} ms):");
            foreach (var cat in result)
            {
                Console.WriteLine($"  Категорія: {cat.Category}");
                foreach (var p in cat.TopProducts)
                {
                    Console.WriteLine($"    - {p.Name}: продано {p.TotalSoldQuantity} шт.");
                }
            }
        }

        // ==========================================
        // ПАЙПЛАЙН 2: Загальний дохід за містами клієнтів
        // ==========================================
        public static void RunPipeline2_RevenueByCustomerCity(List<Sale> sales, List<Product> products, List<Customer> customers)
        {
            var sw = Stopwatch.StartNew();

            // ETL Pipeline: Join продажі + продукти + клієнти -> Розрахунок доходу -> Групування за містами -> Сортування
            var result = sales
                .Join(products, s => s.ProductId, p => p.Id, (s, p) => new { s.CustomerId, Revenue = s.Quantity * p.UnitPrice })
                .Join(customers, sp => sp.CustomerId, c => c.Id, (sp, c) => new { c.City, sp.Revenue })
                .GroupBy(x => x.City)
                .Select(g => new
                {
                    City = g.Key,
                    TotalRevenue = g.Sum(x => x.Revenue)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            sw.Stop();

            Console.WriteLine($"\n[Пайплайн 2] Загальний дохід за містами клієнтів (Час: {sw.ElapsedTicks} ticks / {sw.ElapsedMilliseconds} ms):");
            foreach (var city in result)
            {
                Console.WriteLine($"  Місто: {city.City,-10} | Дохід: {city.TotalRevenue,10:C2}");
            }
        }

        // ==========================================
        // ПАЙПЛАЙН 3: Продажі за місяцями та роками
        // ==========================================
        public static void RunPipeline3_SalesByYearAndMonth(List<Sale> sales, List<Product> products)
        {
            var sw = Stopwatch.StartNew();

            // ETL Pipeline: Join з продуктами -> Групування за роком і місяцем -> Агрегація -> Сортування
            var result = sales
                .Join(products, s => s.ProductId, p => p.Id, (s, p) => new
                {
                    s.SaleDate.Year,
                    s.SaleDate.Month,
                    s.Quantity,
                    Revenue = s.Quantity * p.UnitPrice
                })
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Revenue)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            sw.Stop();

            Console.WriteLine($"\n[Пайплайн 3] Продажі за роками та місяцями (Час: {sw.ElapsedTicks} ticks / {sw.ElapsedMilliseconds} ms):");
            foreach (var item in result)
            {
                Console.WriteLine($"  Рік: {item.Year}, Місяць: {item.Month,2} | Продано одиниць: {item.TotalQuantity,3} | Дохід: {item.TotalRevenue,10:C2}");
            }
        }

        // ==========================================
        // ПАЙПЛАЙН 4 (Власний): Топ активних клієнтів за сумою замовлень
        // ==========================================
        public static void RunPipeline4_TopActiveCustomers(List<Sale> sales, List<Customer> customers)
        {
            var sw = Stopwatch.StartNew();

            // ETL Pipeline: Фільтрація/Групування продажів за клієнтами -> Об'єднання з клієнтами -> Сортування та вибір топ-5
            var result = sales
                .GroupBy(s => s.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    OrderCount = g.Count(),
                    TotalItemsBought = g.Sum(s => s.Quantity)
                })
                .Join(customers,
                    s => s.CustomerId,
                    c => c.Id,
                    (s, c) => new { c.Name, c.City, s.OrderCount, s.TotalItemsBought })
                .OrderByDescending(x => x.TotalItemsBought)
                .Take(5)
                .ToList();

            sw.Stop();

            Console.WriteLine($"\n[Пайплайн 4] Топ-5 найактивніших клієнтів (Час: {sw.ElapsedTicks} ticks / {sw.ElapsedMilliseconds} ms):");
            foreach (var c in result)
            {
                Console.WriteLine($"  Клієнт: {c.Name,-12} ({c.City}) | Замовлень: {c.OrderCount} | Куплено товарів: {c.TotalItemsBought} шт.");
            }
        }
    }
}