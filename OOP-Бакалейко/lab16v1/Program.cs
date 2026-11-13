using System.Diagnostics;
using System.Text;
using lab16v1;

Console.OutputEncoding = Encoding.UTF8;

// 1. Ініціалізація тестових даних (10 категорій, 20 продуктів)
var categories = new List<Category>
{
    new() { Id = 1, Name = "Electronics" },
    new() { Id = 2, Name = "Books" },
    new() { Id = 3, Name = "Clothing" },
    new() { Id = 4, Name = "Home & Kitchen" },
    new() { Id = 5, Name = "Sports" },
    new() { Id = 6, Name = "Toys" },
    new() { Id = 7, Name = "Beauty" },
    new() { Id = 8, Name = "Automotive" },
    new() { Id = 9, Name = "Office" },
    new() { Id = 10, Name = "Garden" }
};

var products = new List<Product>
{
    new() { Id = 1, Name = "Laptop Pro 16", Price = 1850m, CategoryId = 1 },
    new() { Id = 2, Name = "Wireless Mouse", Price = 45m, CategoryId = 1 },
    new() { Id = 3, Name = "Smartphone X", Price = 999m, CategoryId = 1 },
    new() { Id = 4, Name = "4K Monitor", Price = 620m, CategoryId = 1 },
    new() { Id = 5, Name = "USB-C Hub", Price = 80m, CategoryId = 1 },
    new() { Id = 6, Name = "C# in Depth", Price = 55m, CategoryId = 2 },
    new() { Id = 7, Name = "Clean Code", Price = 48m, CategoryId = 2 },
    new() { Id = 8, Name = "Winter Jacket", Price = 140m, CategoryId = 3 },
    new() { Id = 9, Name = "Running Shoes", Price = 110m, CategoryId = 5 },
    new() { Id = 10, Name = "Espresso Machine", Price = 750m, CategoryId = 4 },
    new() { Id = 11, Name = "Blender", Price = 95m, CategoryId = 4 },
    new() { Id = 12, Name = "Yoga Mat", Price = 30m, CategoryId = 5 },
    new() { Id = 13, Name = "Gaming Headset", Price = 160m, CategoryId = 1 },
    new() { Id = 14, Name = "Mechanical Keyboard", Price = 130m, CategoryId = 1 },
    new() { Id = 15, Name = "LEGO Technic Set", Price = 210m, CategoryId = 6 },
    new() { Id = 16, Name = "Facial Serum", Price = 65m, CategoryId = 7 },
    new() { Id = 17, Name = "Car Vacuum Cleaner", Price = 75m, CategoryId = 8 },
    new() { Id = 18, Name = "Ergonomic Chair", Price = 540m, CategoryId = 9 },
    new() { Id = 19, Name = "Lawn Mower", Price = 430m, CategoryId = 10 },
    new() { Id = 20, Name = "Smartwatch Ultra", Price = 799m, CategoryId = 1 }
};

var sw = new Stopwatch();

// =========================================================================
// ЗАПИТ 1: Знайти всі продукти в категорії "Electronics" з ціною > 500
// =========================================================================
Console.WriteLine("=== ЗАПИТ 1: Продукти в категорії 'Electronics' з ціною > 500 ===");

// Синтаксис запитів (Query Syntax)
sw.Restart();
var q1QuerySyntax = (from p in products
                     join c in categories on p.CategoryId equals c.Id
                     where c.Name == "Electronics" && p.Price > 500
                     select p).ToList();
sw.Stop();
Console.WriteLine($"[Query Syntax] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)");
foreach (var p in q1QuerySyntax)
    Console.WriteLine($" - {p.Name}: {p.Price} грн");

// Синтаксис методів (Method Syntax)
sw.Restart();
var q1MethodSyntax = products
    .Join(categories, p => p.CategoryId, c => c.Id, (p, c) => new { Product = p, Category = c })
    .Where(x => x.Category.Name == "Electronics" && x.Product.Price > 500)
    .Select(x => x.Product)
    .ToList();
sw.Stop();
Console.WriteLine($"[Method Syntax] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)\n");


// =========================================================================
// ЗАПИТ 2: Об’єднати продукти з категоріями (Назва продукту + Категорія)
// =========================================================================
Console.WriteLine("=== ЗАПИТ 2: Продукти та їхні категорії ===");

// Синтаксис запитів
sw.Restart();
var q2QuerySyntax = (from p in products
                     join c in categories on p.CategoryId equals c.Id
                     select new { ProductName = p.Name, CategoryName = c.Name }).ToList();
sw.Stop();
Console.WriteLine($"[Query Syntax] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)");
foreach (var item in q2QuerySyntax.Take(5)) // Виводимо перші 5 для компактності
    Console.WriteLine($" - {item.ProductName} -> [{item.CategoryName}]");
Console.WriteLine($" ... (всього {q2QuerySyntax.Count} записів)");

// Синтаксис методів
sw.Restart();
var q2MethodSyntax = products
    .Join(categories,
          p => p.CategoryId,
          c => c.Id,
          (p, c) => new { ProductName = p.Name, CategoryName = c.Name })
    .ToList();
sw.Stop();
Console.WriteLine($"[Method Syntax] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)\n");


// =========================================================================
// ЗАПИТ 3: Підрахувати середню ціну продуктів у кожній категорії
// =========================================================================
Console.WriteLine("=== ЗАПИТ 3: Середня ціна продуктів у кожній категорії ===");

// Синтаксис запитів
sw.Restart();
var q3QuerySyntax = (from c in categories
                     join p in products on c.Id equals p.CategoryId into prodGroup
                     select new
                     {
                         CategoryName = c.Name,
                         AveragePrice = prodGroup.Any() ? prodGroup.Average(x => x.Price) : 0m
                     }).ToList();
sw.Stop();
Console.WriteLine($"[Query Syntax] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)");
foreach (var item in q3QuerySyntax)
    Console.WriteLine($" - {item.CategoryName}: {item.AveragePrice:F2} грн");

// Синтаксис методів (GroupJoin)
sw.Restart();
var q3MethodSyntax = categories
    .GroupJoin(products,
               c => c.Id,
               p => p.CategoryId,
               (c, prodGroup) => new
               {
                   CategoryName = c.Name,
                   AveragePrice = prodGroup.Any() ? prodGroup.Average(x => x.Price) : 0m
               })
    .ToList();
sw.Stop();
Console.WriteLine($"[Method Syntax] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)\n");


// =========================================================================
// ЗАПИТ 4: Знайти найдорожчий продукт
// =========================================================================
Console.WriteLine("=== ЗАПИТ 4: Найдорожчий продукт ===");

// Синтаксис запитів (змішаний підхід, оскільки в чистому Query Syntax немає MaxBy)
sw.Restart();
var q4QuerySyntax = (from p in products
                     orderby p.Price descending
                     select p).FirstOrDefault();
sw.Stop();
Console.WriteLine($"[Query Syntax + FirstOrDefault] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)");
if (q4QuerySyntax != null)
    Console.WriteLine($" - {q4QuerySyntax.Name} ({q4QuerySyntax.Price} грн)");

// Синтаксис методів (оптимальний через MaxBy)
sw.Restart();
var q4MethodSyntax = products.MaxBy(p => p.Price);
sw.Stop();
Console.WriteLine($"[Method Syntax (MaxBy)] Час виконання: {sw.Elapsed.TotalMilliseconds:F4} мс ({sw.ElapsedTicks} тіків)");
if (q4MethodSyntax != null)
    Console.WriteLine($" - {q4MethodSyntax.Name} ({q4MethodSyntax.Price} грн)");
