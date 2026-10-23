using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IndependentWork8
{
    // ==========================================
    // СЦЕНАРІЙ 1: Кеш метаданих (Dictionary)
    // ==========================================
    public class FileMetadata
    {
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Hash { get; set; } = string.Empty;

        public override string ToString() => $"Файл: {FileName}, Розмір: {Size} байт, Змінено: {LastModified:g}";
    }

    public class FileMetadataCache
    {
        private readonly Dictionary<string, FileMetadata> _cache = new Dictionary<string, FileMetadata>(StringComparer.OrdinalIgnoreCase);

        public void Add(string filePath, FileMetadata metadata)
        {
            _cache[filePath] = metadata;
            Console.WriteLine($"[Cache] Додано/оновлено метадані для: {filePath}");
        }

        public FileMetadata Get(string filePath)
        {
            if (_cache.TryGetValue(filePath, out var metadata))
            {
                return metadata;
            }
            throw new KeyNotFoundException($"Файл '{filePath}' відсутній у кеші.");
        }

        public void Remove(string filePath)
        {
            if (_cache.Remove(filePath))
            {
                Console.WriteLine($"[Cache] Видалено з кешу: {filePath}");
            }
        }

        public bool Contains(string filePath)
        {
            return _cache.ContainsKey(filePath);
        }
    }


    // ==========================================
    // СЦЕНАРІЙ 2: Унікальні теги (HashSet)
    // ==========================================
    public class TagManager
    {
        // ArticleId -> Множина унікальних тегів
        private readonly Dictionary<string, HashSet<string>> _articleTags = new Dictionary<string, HashSet<string>>();

        public void AddTag(string articleId, string tag)
        {
            if (!_articleTags.ContainsKey(articleId))
            {
                _articleTags[articleId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            _articleTags[articleId].Add(tag);
            Console.WriteLine($"[TagManager] Додано тег '{tag}' до статті {articleId}");
        }

        public void RemoveTag(string articleId, string tag)
        {
            if (_articleTags.ContainsKey(articleId))
            {
                _articleTags[articleId].Remove(tag);
                Console.WriteLine($"[TagManager] Видалено тег '{tag}' зі статті {articleId}");
            }
        }

        public HashSet<string> GetAllUniqueTags()
        {
            var allTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tags in _articleTags.Values)
            {
                allTags.UnionWith(tags); // Об'єднання множин
            }
            return allTags;
        }

        public HashSet<string> FindCommonTags(string articleId1, string articleId2)
        {
            if (!_articleTags.ContainsKey(articleId1) || !_articleTags.ContainsKey(articleId2))
            {
                return new HashSet<string>();
            }

            // Створюємо копію тегів першої статті
            var common = new HashSet<string>(_articleTags[articleId1], StringComparer.OrdinalIgnoreCase);
            // Залишаємо лише ті, що є у другій статті (перетин)
            common.IntersectWith(_articleTags[articleId2]);
            return common;
        }
    }


    // ==========================================
    // СЦЕНАРІЙ 3: Індекс для пошуку (Dictionary + HashSet)
    // ==========================================
    public class SearchIndex
    {
        // Слово -> Множина ID документів, де воно зустрічається
        private readonly Dictionary<string, HashSet<int>> _invertedIndex = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

        public void AddDocument(int docId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            // Розбиваємо текст на слова (очищаємо від розділових знаків)
            var words = content.Split(new[] { ' ', ',', '.', '!', '?', ';', ',', ':', '-', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                string cleanWord = word.Trim().ToLower();
                if (!_invertedIndex.ContainsKey(cleanWord))
                {
                    _invertedIndex[cleanWord] = new HashSet<int>();
                }
                _invertedIndex[cleanWord].Add(docId);
            }
            Console.WriteLine($"[SearchIndex] Документ #{docId} проіндексовано.");
        }

        public HashSet<int> Search(string word)
        {
            string cleanWord = word.Trim().ToLower();
            if (_invertedIndex.TryGetValue(cleanWord, out var docIds))
            {
                return new HashSet<int>(docIds); // Повертаємо копію множини
            }
            return new HashSet<int>(); // Порожня множина, якщо нічого не знайдено
        }

        public HashSet<int> SearchMultiple(params string[] words)
        {
            if (words == null || words.Length == 0) return new HashSet<int>();

            HashSet<int>? resultSet = null;

            foreach (var word in words)
            {
                var matchingDocs = Search(word);
                if (resultSet == null)
                {
                    resultSet = matchingDocs;
                }
                else
                {
                    resultSet.IntersectWith(matchingDocs); // Залишаємо документи, що містять ВСІ слова
                }

                if (resultSet.Count == 0) break; // Якщо перетин пустий, далі можна не шукати
            }

            return resultSet ?? new HashSet<int>();
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==================================================");
            Console.WriteLine(" ДЕМОНСТРАЦІЯ СЦЕНАРІЮ 1: Кеш метаданих (Dictionary)");
            Console.WriteLine("==================================================");
            var fileCache = new FileMetadataCache();
            fileCache.Add("config.json", new FileMetadata { FileName = "config.json", Size = 1024, LastModified = DateTime.Now, Hash = "abc123hash" });
            
            var meta = fileCache.Get("config.json");
            Console.WriteLine($"Отримано з кешу: {meta}");
            Console.WriteLine($"Чи існує файл у кеші? {fileCache.Contains("config.json")}");
            fileCache.Remove("config.json");


            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ДЕМОНСТРАЦІЯ СЦЕНАРІЮ 2: Унікальні теги (HashSet)");
            Console.WriteLine("==================================================");
            var tagManager = new TagManager();
            tagManager.AddTag("art-1", "C#");
            tagManager.AddTag("art-1", "dotnet");
            tagManager.AddTag("art-1", "OOP");
            
            tagManager.AddTag("art-2", "C#");
            tagManager.AddTag("art-2", "Performance");

            var allUnique = tagManager.GetAllUniqueTags();
            Console.WriteLine($"Усі унікальні теги в системі: {string.Join(", ", allUnique)}");

            var commonTags = tagManager.FindCommonTags("art-1", "art-2");
            Console.WriteLine($"Спільні теги між art-1 та art-2: {string.Join(", ", commonTags)}");


            Console.WriteLine("\n==================================================");
            Console.WriteLine(" ДЕМОНСТРАЦІЮ СЦЕНАРІЮ 3: Пошуковий індекс (Dict + HashSet)");
            Console.WriteLine("==================================================");
            var searchIndex = new SearchIndex();
            searchIndex.AddDocument(1, "Об'єктно-орієнтоване програмування базується на класах та об'єктах у C#.");
            searchIndex.AddDocument(2, "C# є потужною мовою програмування для розробки платформи .NET.");
            searchIndex.AddDocument(3, "Вивчення структур даних та алгоритмів є важливим для програмування.");

            string queryWord = "c#";
            var results = searchIndex.Search(queryWord);
            Console.WriteLine($"Документи, що містять слово '{queryWord}': IDs = [{string.Join(", ", results)}]");

            var multiResults = searchIndex.SearchMultiple("c#", "програмування");
            Console.WriteLine($"Документи, що містять ОДНОЧАСНО 'c#' та 'програмування': IDs = [{string.Join(", ", multiResults)}]");

            Console.WriteLine("\n==================================================");
            Console.WriteLine(" РОБОТУ ПРОГРАМИ ЗАВЕРШЕНО");
            Console.WriteLine("==================================================");
        }
    }
}