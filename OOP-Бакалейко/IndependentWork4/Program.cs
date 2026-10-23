using System;

namespace IndependentWork4
{
    // Базовий клас LibraryItem
    public class LibraryItem
    {
        private string _title;
        private int _publicationYear;
        private string _uniqueId; // Додано в пункті 4

        public string Title => _title;
        public int PublicationYear => _publicationYear;
        public string UniqueId => _uniqueId; // Тільки для читання

        // Модифікований конструктор із параметром uniqueId
        public LibraryItem(string title, int publicationYear, string uniqueId)
        {
            _title = title;
            _publicationYear = publicationYear;
            _uniqueId = uniqueId;
        }

        public virtual void DisplayDetails()
        {
            Console.WriteLine($"[LibraryItem] ID: {_uniqueId}, Назва: {_title}, Рік видання: {_publicationYear}");
        }
    }

    // Похідний клас Book
    public class Book : LibraryItem
    {
        private string _author;

        public string Author => _author;

        // Конструктор викликає базовий конструктор з урахуванням унікального ID
        public Book(string title, int publicationYear, string uniqueId, string author) 
            : base(title, publicationYear, uniqueId)
        {
            _author = author;
        }

        public override void DisplayDetails()
        {
            Console.WriteLine($"[Book] ID: {UniqueId}, Назва: {Title}, Автор: {_author}, Рік видання: {PublicationYear}");
        }
    }

    // Похідний клас Journal
    public class Journal : LibraryItem
    {
        private int _issueNumber;

        public int IssueNumber => _issueNumber;

        // Конструктор викликає базовий конструктор з урахуванням унікального ID
        public Journal(string title, int publicationYear, string uniqueId, int issueNumber) 
            : base(title, publicationYear, uniqueId)
        {
            _issueNumber = issueNumber;
        }

        public override void DisplayDetails()
        {
            Console.WriteLine($"[Journal] ID: {UniqueId}, Назва: {Title}, Випуск №: {_issueNumber}, Рік видання: {PublicationYear}");
        }
    }

    // Похідний клас AudioBook (успадковується від Book)
    public class AudioBook : Book
    {
        private string _narrator;

        public string Narrator => _narrator;

        // Конструктор викликає конструктор базового класу Book
        public AudioBook(string title, int publicationYear, string uniqueId, string author, string narrator) 
            : base(title, publicationYear, uniqueId, author)
        {
            _narrator = narrator;
        }

        public override void DisplayDetails()
        
        {
            Console.WriteLine($"[AudioBook] ID: {UniqueId}, Назва: {Title}, Автор: {Author}, Оповідач: {_narrator}, Рік видання: {PublicationYear}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== ДЕМОНСТРАЦІЯ ІЄРАРХІЇ LIBRARY ITEM ===\n");

            // Створення об'єктів для демонстрації
            LibraryItem book = new Book("Кобзар", 1840, "B001", "Тарас Шевченко");
            LibraryItem journal = new Journal("Науковий вісник", 2026, "J001", 42);
            LibraryItem audioBook = new AudioBook("Тіні забутих предків", 1965, "AB001", "Михайло Коцюбинський", "Василь Стус");

            // Виклик DisplayDetails() для кожного об'єкта
            book.DisplayDetails();
            Console.WriteLine();

            journal.DisplayDetails();
            Console.WriteLine();

            audioBook.DisplayDetails();
            Console.WriteLine("\n=== КІНЕЦЬ ДЕМОНСТРАЦІЇ ===");
        }
    }
}