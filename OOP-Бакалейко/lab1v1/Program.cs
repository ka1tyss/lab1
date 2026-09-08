using System;

namespace Lab1
{
    public class Book
    {
        private string title;
        private string author;
        private int year;

        public int Year
        {
            get => year;
            set
            {
                if (value > 0 && value <= DateTime.Now.Year)
                    year = value;
                else
                    Console.WriteLine("Некоректний рік видання!");
            }
        }

        public string Title
        {
            get => title;
            set => title = value;
        }

        public string Author
        {
            get => author;
            set => author = value;
        }

        public Book(string title, string author, int year)
        {
            this.title = title;
            this.author = author;
            Year = year;
        }
        ~Book()
        {
            Console.WriteLine($"[Деструктор]: Об'єкт \"{title}\" видалено.");
        }
        public string GetInfo()
        {
            return $"Книга: \"{title}\" | Автор: {author} | Рік: {year}";
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== Лабораторна робота №1: Клас Book ===\n");

            Book book1 = new Book("Кобзар", "Тарас Шевченко", 1840);
            Book book2 = new Book("Тіні забутих предків", "Михайло Коцюбинський", 1911);
            Book book3 = new Book("Захар Беркут", "Іван Франко", 1883);

            Console.WriteLine(book1.GetInfo());
            Console.WriteLine(book2.GetInfo());
            Console.WriteLine(book3.GetInfo());

            Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
            Console.ReadKey();
        }
    }
}

