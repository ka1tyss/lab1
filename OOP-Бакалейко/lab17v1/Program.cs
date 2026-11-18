using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab17v1
{
    // 1. Оголошення сутностей
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Book> Books { get; set; } = new();
    }

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<Chapter> Chapters { get; set; } = new();
    }

    public class Chapter
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int PageCount { get; set; }
    }

    // 2. Створення власних методів розширення (Custom Extension Methods)
    public static class ExtensionMethods
    {
        // Метод розширення для IEnumerable<Chapter>, що обчислює загальну кількість сторінок
        public static int TotalPages(this IEnumerable<Chapter> chapters)
        {
            return chapters.Sum(c => c.PageCount);
        }

        // Додатковий корисний метод розширення для красивого виведення в консоль
        public static void PrintTableHeaders(this string title)
        {
            Console.WriteLine($"\n--- {title} ---");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 3. Заповнення колекцій тестовими даними (мінімум 10-20 елементів загалом)
            List<Author> authors = new List<Author>
            {
                new Author
                {
                    Id = 1,
                    Name = "Джордж Мартін",
                    Books = new List<Book>
                    {
                        new Book
                        {
                            Id = 101, Title = "Гра престолів",
                            Chapters = new List<Chapter>
                            {
                                new Chapter { Id = 1, Title = "Вступ", PageCount = 15 },
                                new Chapter { Id = 2, Title = "Бран", PageCount = 25 },
                                new Chapter { Id = 3, Title = "Кетлін", PageCount = 30 }
                            }
                        },
                        new Book
                        {
                            Id = 102, Title = "Битва королів",
                            Chapters = new List<Chapter>
                            {
                                new Chapter { Id = 4, Title = "Ар'я", PageCount = 20 },
                                new Chapter { Id = 5, Title = "Тіріон", PageCount = 35 }
                            }
                        }
                    }
                },
                new Author
                {
                    Id = 2,
                    Name = "Джон Р. Р. Толкін",
                    Books = new List<Book>
                    {
                        new Book
                        {
                            Id = 201, Title = "Братство Персня",
                            Chapters = new List<Chapter>
                            {
                                new Chapter { Id = 6, Title = "Гість із далекого краю", PageCount = 18 },
                                new Chapter { Id = 7, Title = "Тінь минулого", PageCount = 28 },
                                new Chapter { Id = 8, Title = "Трійка компаньйонів", PageCount = 22 }
                            }
                        },
                        new Book
                        {
                            Id = 202, Title = "Дві вежі",
                            Chapters = new List<Chapter>
                            {
                                new Chapter { Id = 9, Title = "Зникнення Бороміра", PageCount = 24 },
                                new Chapter { Id = 10, Title = "Шлях до Ізенгарду", PageCount = 32 }
                            }
                        }
                    }
                }
            };

            // ==========================================
            // ЗАПИТ 1: SelectMany (Розгортання вкладених колекцій)
            // ==========================================
            "ЗАПИТ 1: Усі розділи всіх книг усіх авторів (SelectMany)".PrintTableHeaders();

            // Використовуємо подвійний SelectMany, щоб дістатись до розділів через книги
            var allChapters = authors
                .SelectMany(author => author.Books, (author, book) => new { author, book })
                .SelectMany(ab => ab.book.Chapters, (ab, chapter) => new {
                    AuthorName = ab.author.Name,
                    BookTitle = ab.book.Title,
                    ChapterTitle = chapter.Title,
                    chapter.PageCount
                })
                .ToList();

            foreach (var item in allChapters)
            {
                Console.WriteLine($"Автор: {item.AuthorName,-18} | Книга: {item.BookTitle,-18} | Розділ: {item.ChapterTitle,-22} | Ст.: {item.PageCount}");
            }
            Console.WriteLine($"Всього розділів знайдено: {allChapters.Count}");


            // ==========================================
            // ЗАПИТ 2: Пагінація (Skip та Take)
            // ==========================================
            "ЗАПИТ 2: Пагінація книг (по 3 книги на сторінку)".PrintTableHeaders();

            // Збираємо абсолютно всі книги всіх авторів в один загальний список
            var allBooks = authors.SelectMany(a => a.Books).ToList();
            
            int pageSize = 3;
            int totalBooks = allBooks.Count;
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
            {
                Console.WriteLine($"\n--- Сторінка {pageNumber} з {totalPages} ---");
                
                var booksOnPage = allBooks
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                foreach (var book in booksOnPage)
                {
                    // Демонстрація роботи кастомного методу розширення TotalPages() для розділів конкретної книги
                    int bookPages = book.Chapters.TotalPages();
                    Console.WriteLine($"- Книга: \"{book.Title}\" (Розділів: {book.Chapters.Count}, Сторінок загалом: {bookPages})");
                }
            }


            // ==========================================
            // ЗАПИТ 3: Custom Extension Method (TotalPages)
            // ==========================================
            "ЗАПИТ 3: Демонстрація Custom Extension Method (TotalPages)".PrintTableHeaders();

            // Обчислюємо загальну кількість сторінок по всіх розділах усіх книг (використовуючи наш метод розширення)
            int totalLibraryPages = allChapters.Select(c => new Chapter { PageCount = c.PageCount }).TotalPages();
            Console.WriteLine($"Загальна кількість сторінок у всій бібліотеці: {totalLibraryPages}");

            // Або для окремого автора:
            var firstAuthorChapters = authors.First().Books.SelectMany(b => b.Chapters);
            Console.WriteLine($"Загальна кількість сторінок книг автора '{authors.First().Name}': {firstAuthorChapters.TotalPages()}");
        }
    }
}