using System;

namespace Lab2
{
    public class Book
    {
        private int _year;

        public string Title { get; set; }
        public string Author { get; set; }

        public int Year
        {
            get => _year;
            set
            {
                int currentYear = DateTime.Now.Year;
        
                _year = (value > currentYear) ? currentYear : value;
            }
        }
        public Book() : this("Невідомо", "Невідомо", DateTime.Now.Year) { }

        public Book(string title, string author, int year)
        {
            Title = title;
            Author = author;
            Year = year;
            Console.WriteLine($"[Створено] \"{Title}\"");
        }

        public string GetFullInfo() => $"Книга: \"{Title}\" | Автор: {Author} | Рік: {Year}";

        ~Book() => Console.WriteLine($"[Деструктор] \"{Title}\" видалено.");
    }

    internal class Program
    {
        static void CreateBooks()
        {
            Book book1 = new Book();
            Book book2 = new Book("Кобзар", "Тарас Шевченко", 1840);
            Book book3 = new Book("Тестінг 2099", "Іван Іваненко", 2099);

            Console.WriteLine($"\n{book1.GetFullInfo()}\n{book2.GetFullInfo()}\n{book3.GetFullInfo()}\n");
        }

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            CreateBooks();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}