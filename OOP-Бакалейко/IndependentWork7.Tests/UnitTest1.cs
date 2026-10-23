using System;
using System.Collections.Generic;
using IndependentWork7;
using Xunit;

namespace IndependentWork7.Tests
{
    public class CacheTests
    {
        [Fact]
        public void Add_And_Count_ShouldWorkCorrectly()
        {
            // Arrange
            var cache = new Cache<string, string>(3);

            // Act
            cache.Add("A", "Apple");
            cache.Add("B", "Banana");

            // Assert
            Assert.Equal(2, cache.Count);
        }

        [Fact]
        public void Get_ExistingKey_ShouldReturnCorrectValue()
        {
            // Arrange
            var cache = new Cache<int, string>(2);
            cache.Add(10, "Ten");

            // Act
            string result = cache.Get(10);

            // Assert
            Assert.Equal("Ten", result);
        }

        [Fact]
        public void Get_NonExistingKey_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var cache = new Cache<string, int>(2);

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => cache.Get("Missing"));
        }

        [Fact]
        public void Add_ExceedingCapacity_ShouldRemoveOldestElementFIFO()
        {
            // Arrange: ємність кешу = 2
            var cache = new Cache<int, string>(2);
            cache.Add(1, "First");
            cache.Add(2, "Second");

            // Act: додаємо третій елемент, що перевищує ємність
            cache.Add(3, "Third");

            // Assert: ключ 1 (найстаріший) має бути видалений, а 2 і 3 — залишитися
            Assert.False(cache.ContainsKey(1));
            Assert.True(cache.ContainsKey(2));
            Assert.True(cache.ContainsKey(3));
            Assert.Equal(2, cache.Count);
        }

        [Fact]
        public void ContainsKey_ShouldReturnExpectedResults()
        {
            // Arrange
            var cache = new Cache<string, string>(3);
            cache.Add("Key1", "Value1");

            // Act & Assert
            Assert.True(cache.ContainsKey("Key1"));
            Assert.False(cache.ContainsKey("NonExistent"));
        }
    }
}
