using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class CountryServiceTests
    {
        private readonly Mock<ICountryRepository> _countryRepositoryMock;
        private readonly CountryService _service;

        public CountryServiceTests()
        {
            _countryRepositoryMock = new Mock<ICountryRepository>();

            _service = new CountryService(
                _countryRepositoryMock.Object);
        }

        [Fact]
        public async Task Create_AddsAndSaves()
        {
            // Arrange
            var country = new Country();

            // Act
            await _service.Create(country);

            // Assert
            _countryRepositoryMock.Verify(
                x => x.Add(country),
                Times.Once);

            _countryRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesAndSaves()
        {
            // Arrange
            var country = new Country();

            // Act
            await _service.Delete(country);

            // Assert
            _countryRepositoryMock.Verify(
                x => x.Remove(country),
                Times.Once);

            _countryRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<Country>
            {
                new Country(),
                new Country()
            };

            _countryRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetAll();

            // Assert
            result.Should().BeSameAs(expected);

            _countryRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsRepositoryResult()
        {
            // Arrange
            const int countryId = 5;
            var expected = new Country();

            _countryRepositoryMock
                .Setup(x => x.GetById(countryId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetById(countryId);

            // Assert
            result.Should().BeSameAs(expected);

            _countryRepositoryMock.Verify(
                x => x.GetById(countryId),
                Times.Once);
        }

        [Fact]
        public async Task Update_UpdatesAndSaves()
        {
            // Arrange
            var country = new Country();

            // Act
            await _service.Update(country);

            // Assert
            _countryRepositoryMock.Verify(
                x => x.Update(country),
                Times.Once);

            _countryRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetCountriesAsSelectList_ReturnsCountriesWithCorrectSelection()
        {
            // Arrange
            var countries = new List<Country>
            {
                new Country
                {
                    CountryId = 1,
                    CountryNameLong = "Nederland"
                },
                new Country
                {
                    CountryId = 2,
                    CountryNameLong = "België"
                }
            };

            _countryRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(countries);

            // Act
            var result = (await _service.GetCountriesAsSelectList(2)).ToList();

            // Assert
            result.Should().HaveCount(2);

            result[0].Value.Should().Be("1");
            result[0].Text.Should().Be("Nederland");
            result[0].Selected.Should().BeFalse();

            result[1].Value.Should().Be("2");
            result[1].Text.Should().Be("België");
            result[1].Selected.Should().BeTrue();

            _countryRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Once);
        }
    }
}