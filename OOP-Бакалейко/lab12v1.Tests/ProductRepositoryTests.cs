using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using lab12v1;

namespace lab12v1.Tests
{
    public class ProductRepositoryTests
    {
        [Fact]
        public void Add_ShouldAddNewItem()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();
            var product = new Product { Id = 1, Name = "Keyboard", Price = 1500m, Stock = 10 };

            // Act
            repo.Add(product);
            var result = repo.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Keyboard", result.Name);
            Assert.Equal(1500m, result.Price);
        }

        [Fact]
        public void GetById_ShouldReturnCorrectItem_WhenExists()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();
            repo.Add(new Product { Id = 10, Name = "Monitor", Price = 8000m, Stock = 3 });

            // Act
            var result = repo.GetById(10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Monitor", result.Name);
        }

        [Fact]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();

            // Act
            var result = repo.GetById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAll_ShouldReturnAllItems()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();
            repo.Add(new Product { Id = 1, Name = "A", Price = 10m, Stock = 1 });
            repo.Add(new Product { Id = 2, Name = "B", Price = 20m, Stock = 2 });

            // Act
            var items = repo.GetAll().ToList();

            // Assert
            Assert.Equal(2, items.Count);
            Assert.Contains(items, p => p.Id == 1);
            Assert.Contains(items, p => p.Id == 2);
        }

        [Fact]
        public void Update_ShouldModifyExistingItem()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();
            repo.Add(new Product { Id = 1, Name = "OldName", Price = 100m, Stock = 5 });

            // Act
            var updatedProduct = new Product { Id = 1, Name = "NewName", Price = 150m, Stock = 8 };
            repo.Update(updatedProduct);
            var result = repo.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewName", result.Name);
            Assert.Equal(150m, result.Price);
            Assert.Equal(8, result.Stock);
        }

        [Fact]
        public void Update_ShouldThrowKeyNotFoundException_WhenItemNotExists()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();
            var nonExistent = new Product { Id = 99, Name = "Ghost", Price = 0m, Stock = 0 };

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => repo.Update(nonExistent));
        }

        [Fact]
        public void Delete_ShouldRemoveItem_WhenExists()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();
            repo.Add(new Product { Id = 5, Name = "ItemToDelete", Price = 50m, Stock = 2 });

            // Act
            repo.Delete(5);
            var result = repo.GetById(5);

            // Assert
            Assert.Null(result);
            Assert.Empty(repo.GetAll());
        }

        [Fact]
        public void Delete_ShouldThrowKeyNotFoundException_WhenItemNotExists()
        {
            // Arrange
            IRepository<Product> repo = new InMemoryRepository<Product>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => repo.Delete(404));
        }
    }
}