using System;

public class Book
{
    private string _title;
    private int _pagesCount;
    private double _price;

    public string Title
    {
        get { return _title; }
        set { _title = value; }
    }

    public int PagesCount
    {
        get { return _pagesCount; }
    }

    public double Price
    {
        get { return _price; }
        set 
        { 
            if (value >= 0) 
                _price = value; 
        }
    }

    public Book(string title, int pagesCount, double price)
    {
        _title = title;
        _pagesCount = pagesCount;
        _price = price;
    }

    public double EstimateReadingTimeHours()
    {
        double totalMinutes = _pagesCount * 2.0;
        return totalMinutes / 60.0;
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Книга: '{_title}', Сторінок: {_pagesCount}, Ціна: {_price:F2} грн");
    }
}

public class Car
{
    private string _model;
    private double _fuelLevel;
    private double _fuelConsumption;

    public string Model
    {
        get { return _model; }
        set { _model = value; }
    }

    public double FuelLevel
    {
        get { return _fuelLevel; }
        private set { _fuelLevel = value; }
    }

    public Car(string model, double fuelLevel, double fuelConsumption)
    {
        _model = model;
        _fuelLevel = fuelLevel;
        _fuelConsumption = fuelConsumption;
    }

    public bool TryDrive(double distance)
    {
        double requiredFuel = (distance / 100.0) * _fuelConsumption;
        if (_fuelLevel >= requiredFuel)
        {
            _fuelLevel -= requiredFuel;
            return true;
        }
        return false;
    }

    public void PrintStatus()
    {
        Console.WriteLine($"Автомобіль: {_model} | Залишок пального: {_fuelLevel:F2} л");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== Самостійна робота №1: Базовий синтаксис C# ===\n");

        Console.WriteLine("--- Тестування класу Book ---");
        Book myBook = new Book("Об'єктно-орієнтоване програмування в C#", 380, 550.00);
        myBook.PrintInfo();

        double readingTime = myBook.EstimateReadingTimeHours();
        Console.WriteLine($"Орієнтовний час на прочитання: {readingTime:F1} год.\n");

        myBook.Price = 499.00;
        Console.WriteLine($"Нова акційна ціна книги: {myBook.Price:F2} грн\n");

        Console.WriteLine("--- Тестування класу Car ---");
        Car myCar = new Car("Toyota Corolla", 45.0, 7.5);
        myCar.PrintStatus();

        double tripDistance = 350.0;
        Console.WriteLine($"Спроба здійснити поїздку на {tripDistance} км...");

        bool success = myCar.TryDrive(tripDistance);
        if (success)
        {
            Console.WriteLine("Поїздка успішна!");
        }
        else
        {
            Console.WriteLine("Поїздка неможлива: недостатньо пального!");
        }

        myCar.PrintStatus();
    }
}