using System;

public class Product
{
    // Приватні поля
    private int _id;
    private string _name;
    private decimal _price;
    private string _category;
    private int _stockCount;

    // Публічні read-only властивості
    public int Id 
    { 
        get { return _id; } 
    }

    public string Name 
    { 
        get { return _name; } 
    }

    public decimal Price 
    { 
        get { return _price; } 
    }

    public string Category 
    { 
        get { return _category; } 
    }

    public int StockCount 
    { 
        get { return _stockCount; } 
    }

    // 1. Основний конструктор (приймає всі 5 параметрів)
    public Product(int id, string name, decimal price, string category, int stockCount)
    {
        _id = id;
        _name = name;
        _price = price;
        _category = category;
        _stockCount = stockCount;
    }

    // 2. Скорочений конструктор для швидкого додавання (викликає основний через : this)
    public Product(int id, string name, decimal price) 
        : this(id, name, price, "Uncategorized", 0)
    {
    }

    // 3. Конструктор копіювання (створює дублікат іншого об'єкта Product)
    public Product(Product other) 
        : this(other._id, other._name, other._price, other._category, other._stockCount)
    {
    }

    // Перевизначення методу ToString()
    public override string ToString()
    {
        return $"ID: {Id}, Name: {Name}, Price: {Price:C}, Category: {Category}, Stock: {StockCount}";
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // Налаштування кодування для коректного відображення символу валюти (₴) та кирилиці
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Створення товарів\n");

        // Використання основного конструктора
        Product product1 = new Product(101, "Laptop", 35000.00m, "Electronics", 15);
        Console.WriteLine($"Товар 1 (основний конструктор): {product1}");

        // Використання скороченого конструктора
        Product product2 = new Product(102, "Mouse", 800.00m);
        Console.WriteLine($"Товар 2 (скорочений конструктор): {product2}");

        // Використання конструктора копіювання
        Product product3 = new Product(product1);
        Console.WriteLine($"Товар 3 (конструктор копіювання): {product3}");
    }
}
