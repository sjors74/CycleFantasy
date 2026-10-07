using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class SeasonYearRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public SeasonYearRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task AddAsync_AddsSeasonYear()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            var year = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            await repo.AddAsync(year);
            await context.SaveChangesAsync();

            var result = await context.SeasonYears.FindAsync(1);

            result.Should().NotBeNull();
            result!.Year.Should().Be(2026);
        }

        [Fact]
        public async Task Delete_RemovesSeasonYear()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            var year = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            };

            context.SeasonYears.Add(year);
            await context.SaveChangesAsync();

            repo.Delete(year);
            await context.SaveChangesAsync();

            var result = await context.SeasonYears.FindAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenYearExists()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            context.SeasonYears.Add(new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            });

            await context.SaveChangesAsync();

            var result = await repo.ExistsAsync(2026);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenYearDoesNotExist()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            context.SeasonYears.Add(new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            });

            await context.SaveChangesAsync();

            var result = await repo.ExistsAsync(2025);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSeasonYearsOrderedDescending()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            context.SeasonYears.AddRange(
                new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2024
                },
                new SeasonYear
                {
                    SeasonYearId = 2,
                    Year = 2026
                },
                new SeasonYear
                {
                    SeasonYearId = 3,
                    Year = 2025
                });

            await context.SaveChangesAsync();

            var results = await repo.GetAllAsync();

            results.Should().HaveCount(3);
            results.Select(x => x.Year)
                .Should()
                .Equal(2026, 2025, 2024);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSeasonYear_WhenExists()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            context.SeasonYears.Add(new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026
            });

            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.SeasonYearId.Should().Be(1);
            result.Year.Should().Be(2026);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            var result = await repo.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_UpdatesSeasonYear()
        {
            using var context = CreateContext();
            var repo = new SeasonYearRepository(context);

            context.SeasonYears.Add(new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2025
            });

            await context.SaveChangesAsync();

            var year = await context.SeasonYears.FindAsync(1);
            year.Should().NotBeNull();

            year!.Year = 2026;

            repo.Update(year);
            await context.SaveChangesAsync();

            var result = await context.SeasonYears.FindAsync(1);

            result.Should().NotBeNull();
            result!.Year.Should().Be(2026);
        }
    }
}