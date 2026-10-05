using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class SeasonYearServiceTests
    {
        private readonly Mock<ISeasonYearRepository> _repositoryMock;
        private readonly SeasonYearService _service;

        public SeasonYearServiceTests()
        {
            _repositoryMock = new Mock<ISeasonYearRepository>();
            _service = new SeasonYearService(_repositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_AddsAndSaves_WhenYearDoesNotExist()
        {
            // Arrange
            var year = new SeasonYear
            {
                Year = 2026
            };

            _repositoryMock
                .Setup(x => x.ExistsAsync(2026))
                .ReturnsAsync(false);

            // Act
            await _service.CreateAsync(year);

            // Assert
            _repositoryMock.Verify(
                x => x.ExistsAsync(2026),
                Times.Once);

            _repositoryMock.Verify(
                x => x.AddAsync(year),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenYearAlreadyExists()
        {
            // Arrange
            var year = new SeasonYear
            {
                Year = 2026
            };

            _repositoryMock
                .Setup(x => x.ExistsAsync(2026))
                .ReturnsAsync(true);

            // Act
            var act = () => _service.CreateAsync(year);

            // Assert
            var exception = await act.Should()
                .ThrowAsync<InvalidOperationException>();

            exception.Which.Message
                .Should()
                .Be("Jaar 2026 bestaat al.");

            _repositoryMock.Verify(
                x => x.AddAsync(It.IsAny<SeasonYear>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenYearDoesNotExist()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((SeasonYear?)null);

            // Act
            await _service.DeleteAsync(999);

            // Assert
            _repositoryMock.Verify(
                x => x.GetByIdAsync(999),
                Times.Once);

            _repositoryMock.Verify(
                x => x.Update(It.IsAny<SeasonYear>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_SetsYearInactiveAndSaves()
        {
            // Arrange
            var year = new SeasonYear
            {
                SeasonYearId = 5,
                Year = 2026,
                Active = true
            };

            _repositoryMock
                .Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(year);

            // Act
            await _service.DeleteAsync(5);

            // Assert
            year.Active.Should().BeFalse();

            _repositoryMock.Verify(
                x => x.GetByIdAsync(5),
                Times.Once);

            _repositoryMock.Verify(
                x => x.Update(year),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedSeasonYears()
        {
            // Arrange
            var years = new List<SeasonYear>
            {
                new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2025,
                    Active = true
                },
                new SeasonYear
                {
                    SeasonYearId = 2,
                    Year = 2026,
                    Active = false
                }
            };

            _repositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(years);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);

            result[0].SeasonYearId.Should().Be(1);
            result[0].Year.Should().Be(2025);
            result[0].Active.Should().BeTrue();

            result[1].SeasonYearId.Should().Be(2);
            result[1].Year.Should().Be(2026);
            result[1].Active.Should().BeFalse();

            _repositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new SeasonYear
            {
                SeasonYearId = 5,
                Year = 2026,
                Active = true
            };

            _repositoryMock
                .Setup(x => x.GetByIdAsync(5))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetByIdAsync(5);

            // Assert
            result.Should().BeSameAs(expected);

            _repositoryMock.Verify(
                x => x.GetByIdAsync(5),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAndSaves()
        {
            // Arrange
            var year = new SeasonYear
            {
                SeasonYearId = 5,
                Year = 2026,
                Active = true
            };

            // Act
            await _service.UpdateAsync(year);

            // Assert
            _repositoryMock.Verify(
                x => x.Update(year),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}