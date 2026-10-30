using System;

namespace lab12v1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Demonstartion of InMemoryRepository<Product> ===");

            IRepository<Product> repository = new InMemoryRepository<Product>();

            // Додавання
            repository.Add(new Product { Id = 1, Name = "Laptop", Price = 35000m, Stock = 5 });
            repository.Add(new Product { Id = 2, Name = "Mouse", Price = 1200m, Stock = 25 });

            // GetAll
            Console.WriteLine("\nAll products after adding:");
            foreach (var p in repository.GetAll())
            {
                Console.WriteLine($"[Id: {p.Id}] {p.Name} - {p.Price} UAH (Stock: {p.Stock})");
            }

            // GetById
            var product = repository.GetById(1);
            Console.WriteLine($"\nGetById(1): {product?.Name}");

            // Update
            if (product != null)
            {
                product.Price = 33000m;
                product.Stock = 4;
                repository.Update(product);
                Console.WriteLine($"\nUpdated Product 1 Price: {repository.GetById(1)?.Price} UAH");
            }

            // Delete
            repository.Delete(2);
            Console.WriteLine("\nAll products after deleting Id 2:");
            foreach (var p in repository.GetAll())
            {
                Console.WriteLine($"[Id: {p.Id}] {p.Name}");
            }
        }
    }
}