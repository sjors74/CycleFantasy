using CycleManager.Domain.Models;
using DataAccessEF.TypeRepository;
using Domain.Context;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CycleManager.Tests.Integration.DataAccess
{
    public class UserRepositoryIntegrationTests
    {
        private readonly ServiceProvider _serviceProvider;

        public UserRepositoryIntegrationTests()
        {
            var services = new ServiceCollection();

            // Gebruik InMemory EF Core database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("UserRepositoryIntegrationTests"));

            // Voeg logging toe (voor UserManager)
            services.AddLogging();

            // Voeg Identity toe (met ApplicationUser)
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            _serviceProvider = services.BuildServiceProvider();
        }

        private UserRepository CreateRepository()
        {
            var context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            return new UserRepository(context, userManager);
        }

        [Fact]
        public async Task GetConfirmedUsersAsync_ReturnsOnlyConfirmedUsers()
        {
            // Arrange
            var repo = CreateRepository();
            var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user1 = new ApplicationUser { UserName = "confirmed1", Email = "a@test.com", EmailConfirmed = true, FirstName = "a", LastName = "Tester" };
            var user2 = new ApplicationUser {UserName = "unconfirmed", Email = "b@test.com", EmailConfirmed = false, FirstName = "b", LastName = "Tester" };
            var user3 = new ApplicationUser {UserName = "confirmed2", Email = "c@test.com", EmailConfirmed = true, FirstName = "c", LastName = "Tester" };

            await userManager.CreateAsync(user1, "Password123!");
            await userManager.CreateAsync(user2, "Password123!");
            await userManager.CreateAsync(user3, "Password123!");

            // Act
            var result = await repo.GetConfirmedUsersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(u => u.EmailConfirmed);
        }

        [Fact]
        public async Task GetConfirmedUsersAsync_ReturnsEmpty_WhenNoConfirmedUsers()
        {
            // Arrange
            var repo = CreateRepository();
            var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = new ApplicationUser { UserName = "unconfirmed", Email = "x@test.com", EmailConfirmed = false, FirstName = "x", LastName = "Tester" };
            await userManager.CreateAsync(user, "Password123!");

            // Act
            var result = await repo.GetConfirmedUsersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConfirmedUsersAsync_ReturnsEmpty_WhenNoUsersAtAll()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetConfirmedUsersAsync();

            // Assert
            result.Should().BeEmpty("er zijn geen gebruikers in de database");
        }
    }
}
