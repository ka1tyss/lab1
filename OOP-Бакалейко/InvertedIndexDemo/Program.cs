using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InvertedIndexDemo
{
    // Клас документа
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public override string ToString() => $"[{Id}] {Title}";
    }

    // Утилітний клас для обробки тексту
    public static class TextProcessor
    {
        /// <summary>
        /// Розбиває текст на слова, видаляє пунктуацію, переводить у нижній регістр
        /// </summary>
        public static IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<string>();

            // Використовуємо регулярний вираз для виділення слів (або \w+, що відповідає словам та цифрам)
            var matches = Regex.Matches(text.ToLowerInvariant(), @"\w+");
            var words = new List<string>();
            foreach (Match match in matches)
            {
                words.Add(match.Value);
            }
            return words;
        }

        /// <summary>
        /// Видаляє стоп-слова
        /// </summary>
        public static IEnumerable<string> RemoveStopWords(IEnumerable<string> words)
        {
            var stopWords = new HashSet<string>
            {
                "a", "an", "the", "is", "are", "was", "were",
                "in", "on", "at", "to", "for", "of", "and", "or"
            };
            return words.Where(w => !stopWords.Contains(w));
        }
    }

    // Клас інвертованого індексу
    public class InvertedIndex
    {
        private readonly Dictionary<string, HashSet<int>> _index;
        private readonly Dictionary<int, Document> _documents;

        public InvertedIndex()
        {
            _index = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
            _documents = new Dictionary<int, Document>();
        }

        /// <summary>
        /// Додає документ до індексу
        /// </summary>
        public void AddDocument(Document doc)
        {
            if (doc == null) return;

            _documents[doc.Id] = doc;

            // Токенізуємо контент та видаляємо стоп-слова для якіснішого індексування
            var tokens = TextProcessor.Tokenize(doc.Content);
            var filteredWords = TextProcessor.RemoveStopWords(tokens);

            foreach (var word in filteredWords)
            {
                if (!_index.ContainsKey(word))
                {
                    _index[word] = new HashSet<int>();
                }
                _index[word].Add(doc.Id);
            }
        }

        /// <summary>
        /// Шукає документи, що містять слово
        /// </summary>
        public List<Document> SearchSingleWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new List<Document>();

            string cleanWord = word.Trim().ToLowerInvariant();

            if (_index.TryGetValue(cleanWord, out var docIds))
            {
                return docIds.Select(id => _documents[id]).ToList();
            }

            return new List<Document>();
        }

        /// <summary>
        /// Шукає документи, що містять ВСІ слова (AND)
        /// </summary>
        public List<Document> SearchMultipleWords(params string[] words)
        {
            if (words == null || words.Length == 0)
                return new List<Document>();

            HashSet<int>? resultIds = null;

            foreach (var word in words)
            {
                string cleanWord = word.Trim().ToLowerInvariant();
                
                // Отримуємо множину ID для поточного слова (або порожню, якщо слова нема)
                HashSet<int> currentDocs;
                if (_index.TryGetValue(cleanWord, out var docIds))
                {
                    currentDocs = docIds;
                }
                else
                {
                    return new List<Document>(); // Якщо хоча б одного слова немає, перетин буде порожнім
                }

                if (resultIds == null)
                {
                    resultIds = new HashSet<int>(currentDocs);
                }
                else
                {
                    resultIds.IntersectWith(currentDocs);
                }

                if (resultIds.Count == 0)
                {
                    break;
                }
            }

            if (resultIds == null)
                return new List<Document>();

            return resultIds.Select(id => _documents[id]).ToList();
        }

        /// <summary>
        /// Шукає документи, що містять БУДЬ-ЯКЕ слово (OR)
        /// </summary>
        public List<Document> SearchAnyWord(params string[] words)
        {
            if (words == null || words.Length == 0)
                return new List<Document>();

            var resultIds = new HashSet<int>();

            foreach (var word in words)
            {
                string cleanWord = word.Trim().ToLowerInvariant();
                if (_index.TryGetValue(cleanWord, out var docIds))
                {
                    resultIds.UnionWith(docIds);
                }
            }

            return resultIds.Select(id => _documents[id]).ToList();
        }

        public int DocumentCount => _documents.Count;
        public int UniqueWords => _index.Keys.Count;
    }

    // Клас для порівняння продуктивності
    public class PerformanceTest
    {
        public static void CompareSearchMethods(List<Document> documents, string searchWord)
        {
            Console.WriteLine($"\n--- Порівняння продуктивності для запиту: '{searchWord}' (Документів: {documents.Count}) ---");
            
            var sw = new Stopwatch();

            // 1. Лінійний пошук
            sw.Start();
            var normalizedWord = searchWord.ToLowerInvariant();
            var linearResults = documents.Where(d =>
                TextProcessor.Tokenize(d.Content).Contains(normalizedWord)).ToList();
            sw.Stop();
            long linearTicks = sw.ElapsedTicks;
            Console.WriteLine($"Лінійний пошук (Linear Search): {linearTicks} ticks ({sw.ElapsedMilliseconds} ms)");

            // 2. Inverted Index (створення індексу та пошук)
            sw.Restart();
            var index = new InvertedIndex();
            foreach (var doc in documents)
            {
                index.AddDocument(doc);
            }
            var indexResults = index.SearchSingleWord(searchWord);
            sw.Stop();
            long indexTicks = sw.ElapsedTicks;
            Console.WriteLine($"Інвертований індекс (Inverted Index): {indexTicks} ticks ({sw.ElapsedMilliseconds} ms)");

            // Перевірка коректності
            Console.WriteLine($"Результати: Лінійний знайшов {linearResults.Count}, Індекс знайшов {indexResults.Count}");
        }

        public static List<Document> GenerateTestDocuments(int count)
        {
            var sampleTexts = new[]
            {
                "C# is a modern object-oriented programming language developed by Microsoft.",
                "Java is a popular programming language known for its portability and performance.",
                "Python is a high-level programming language with simple syntax.",
                "C# supports LINQ, async/await, and many modern programming paradigms.",
                "C#, Java, and Python are among the most popular programming languages today."
            };

            var docs = new List<Document>();
            var rand = new Random(42);

            for (int i = 1; i <= count; i++)
            {
                string text = sampleTexts[rand.Next(sampleTexts.Length)];
                docs.Add(new Document
                {
                    Id = i,
                    Title = $"Document #{i}",
                    Content = $"{text} Additional random data token {rand.Next(1000)}."
                });
            }

            return docs;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Базовий набір тестових документів із завдання
            var initialDocuments = new List<Document>
            {
                new Document
                {
                    Id = 1,
                    Title = "Introduction to C#",
                    Content = "C# is a modern object-oriented programming language developed by Microsoft."
                },
                new Document
                {
                    Id = 2,
                    Title = "Java Basics",
                    Content = "Java is a popular programming language known for its portability and performance."
                },
                new Document
                {
                    Id = 3,
                    Title = "Python Overview",
                    Content = "Python is a high-level programming language with simple syntax."
                },
                new Document
                {
                    Id = 4,
                    Title = "C# Advanced Features",
                    Content = "C# supports LINQ, async/await, and many modern programming paradigms."
                },
                new Document
                {
                    Id = 5,
                    Title = "Comparing Languages",
                    Content = "C#, Java, and Python are among the most popular programming languages today."
                }
            };

            // Запуск тестів продуктивності для різної кількості документів
            Console.WriteLine("=== ТЕСТУВАННЯ ПРОДУКТИВНОСТІ ===");
            foreach (var size in new[] { 10, 100, 1000, 10000 })
            {
                var testDocs = PerformanceTest.GenerateTestDocuments(size);
                PerformanceTest.CompareSearchMethods(testDocs, "programming");
            }

            // Інтерактивна демонстрація з початковими документами
            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ІНТЕРАКТИВНА ДЕМОНСТРАЦІЯ ПОШУКУ");
            Console.WriteLine("==================================================");

            var index = new InvertedIndex();
            foreach (var doc in initialDocuments)
            {
                index.AddDocument(doc);
            }

            Console.WriteLine($"Завантажено документів: {index.DocumentCount}");
            Console.WriteLine($"Індекс містить унікальних слів: {index.UniqueWords}\n");

            while (true)
            {
                Console.Write("Введіть пошуковий запит (або 'exit' для виходу): ");
                var query = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(query) || query.Trim().ToLower() == "exit")
                {
                    break;
                }

                var words = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                Console.WriteLine("\n--- Результати пошуку (AND) ---");
                var results = index.SearchMultipleWords(words);

                if (results.Count == 0)
                {
                    Console.WriteLine("Нічого не знайдено за вашим запитом.");
                }
                else
                {
                    foreach (var doc in results)
                    {
                        Console.WriteLine(doc);
                        string preview = doc.Content.Length > 100 ? doc.Content.Substring(0, 100) + "..." : doc.Content;
                        Console.WriteLine($"   {preview}");
                    }
                }
                Console.WriteLine($"\nЗнайдено документів: {results.Count}\n");
            }

            Console.WriteLine("Роботу програми завершено.");
        }
    }
}