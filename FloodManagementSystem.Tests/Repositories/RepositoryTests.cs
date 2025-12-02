using Microsoft.EntityFrameworkCore;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Repositories.Implementations;

namespace GlobalDisasterManagement.Tests.Repositories
{
    public class RepositoryTests
    {
        private DisasterDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DisasterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DisasterDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var cities = new List<City>
            {
                new City { Id = 1, Name = "Lagos Island", LGAId = 1 },
                new City { Id = 2, Name = "Ikeja", LGAId = 2 },
                new City { Id = 3, Name = "Surulere", LGAId = 3 }
            };

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var city = new City { Id = 1, Name = "Lagos Island", LGAId = 1 };
            await context.Cities.AddAsync(city);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Lagos Island", result.Name);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntityToDatabase()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var city = new City { Id = 1, Name = "Victoria Island", LGAId = 1 };

            // Act
            await repository.AddAsync(city);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.Cities.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Victoria Island", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyEntity()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var city = new City { Id = 1, Name = "Lagos Island", LGAId = 1 };
            await context.Cities.AddAsync(city);
            await context.SaveChangesAsync();

            // Act
            city.Name = "Updated Lagos Island";
            repository.Update(city);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.Cities.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Updated Lagos Island", result.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntity()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var city = new City { Id = 1, Name = "Lagos Island", LGAId = 1 };
            await context.Cities.AddAsync(city);
            await context.SaveChangesAsync();

            // Act
            repository.Delete(city);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.Cities.FindAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task FindAsync_WithPredicate_ShouldReturnMatchingEntities()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var cities = new List<City>
            {
                new City { Id = 1, Name = "Lagos Island", LGAId = 1 },
                new City { Id = 2, Name = "Ikeja", LGAId = 2 },
                new City { Id = 3, Name = "Victoria Island", LGAId = 1 }
            };

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.FindAsync(c => c.LGAId == 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.Equal(1, c.LGAId));
        }

        [Fact]
        public async Task CountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var cities = new List<City>
            {
                new City { Id = 1, Name = "City1", LGAId = 1 },
                new City { Id = 2, Name = "City2", LGAId = 1 },
                new City { Id = 3, Name = "City3", LGAId = 2 }
            };

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();

            // Act
            var totalCount = await repository.CountAsync();
            var lgaCount = await repository.CountAsync(c => c.LGAId == 1);

            // Assert
            Assert.Equal(3, totalCount);
            Assert.Equal(2, lgaCount);
        }

        [Fact]
        public async Task AnyAsync_ShouldReturnCorrectBoolean()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var repository = new Repository<City>(context);

            var city = new City { Id = 1, Name = "Lagos Island", LGAId = 1 };
            await context.Cities.AddAsync(city);
            await context.SaveChangesAsync();

            // Act
            var exists = await repository.AnyAsync(c => c.Name == "Lagos Island");
            var notExists = await repository.AnyAsync(c => c.Name == "NonExistent");

            // Assert
            Assert.True(exists);
            Assert.False(notExists);
        }
    }
}
