using System;
using System.Collections.Generic;
using System.Linq;

namespace lab12v1
{
    public class InMemoryRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly List<T> _items = new();

        public T? GetById(int id)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }

        public IEnumerable<T> GetAll()
        {
            return _items.ToList(); // Повертаємо копію для безпеки
        }

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            if (_items.Any(i => i.Id == item.Id))
                throw new InvalidOperationException($"Item with Id {item.Id} already exists.");

            _items.Add(item);
        }

        public void Update(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var existingIndex = _items.FindIndex(i => i.Id == item.Id);
            if (existingIndex == -1)
                throw new KeyNotFoundException($"Item with Id {item.Id} was not found for update.");

            _items[existingIndex] = item;
        }

        public void Delete(int id)
        {
            var item = GetById(id);
            if (item == null)
                throw new KeyNotFoundException($"Item with Id {id} was not found for deletion.");

            _items.Remove(item);
        }
    }
}