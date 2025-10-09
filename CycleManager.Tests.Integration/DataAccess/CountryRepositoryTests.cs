using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class CountryRepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void GetById_ReturnsCorrectCountry()
        {
            // Arrange
            using var context = GetInMemoryDbContext("GetByIdTest");
            var country = new Country { CountryId = 1, CountryNameLong = "Nederland" };
            context.Country.Add(country);
            context.SaveChanges();

            var repo = new CountryRepository(context);

            // Act
            var result = repo.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CountryId);
            Assert.Equal("Nederland", result.CountryNameLong);
        }

        [Fact]
        public void Update_ChangesCountryData()
        {
            // Arrange
            using var context = GetInMemoryDbContext("UpdateTest");
            var country = new Country { CountryId = 1, CountryNameLong = "Nederland" };
            context.Country.Add(country);
            context.SaveChanges();

            var repo = new CountryRepository(context);
            country.CountryNameLong = "België";

            // Act
            repo.Update(country);
            context.SaveChanges();

            // Assert
            var updated = context.Country.FirstOrDefault(c => c.CountryId == 1);
            Assert.NotNull(updated);
            Assert.Equal("België", updated.CountryNameLong);
        }

        [Fact]
        public void Remove_DeletesCountry()
        {
            // Arrange
            using var context = GetInMemoryDbContext("RemoveTest");
            var country = new Country { CountryId = 1, CountryNameLong = "Nederland" };
            context.Country.Add(country);
            context.SaveChanges();

            var repo = new CountryRepository(context);

            // Act
            repo.Remove(country);
            context.SaveChanges();

            // Assert
            var deleted = context.Country.FirstOrDefault(c => c.CountryId == 1);
            Assert.Null(deleted);
        }
    }
}