using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _service = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _userRepositoryMock
                .Setup(x => x.GetConfirmedUsersAsync())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetAllUsers();

            // Assert
            result.Should().BeEquivalentTo(expected);

            _userRepositoryMock.Verify(
                x => x.GetConfirmedUsersAsync(),
                Times.Once);
        }
    }
}