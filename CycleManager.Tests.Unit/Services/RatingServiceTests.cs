using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class RatingServiceTests
    {
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly RatingService _service;

        public RatingServiceTests()
        {
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _service = new RatingService(_ratingRepositoryMock.Object);
        }

        [Fact]
        public async Task GetRatingCategories_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<RatingCategory>
            {
                new RatingCategory()
            };

            _ratingRepositoryMock
                .Setup(x => x.GetRatingCategories())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetRatingCategories();

            // Assert
            result.Should().BeEquivalentTo(expected);

            _ratingRepositoryMock.Verify(
                x => x.GetRatingCategories(),
                Times.Once);
        }

        [Fact]
        public async Task GetRatings_ReturnsRepositoryResult()
        {
            // Arrange
            var expected = new List<CompetitorRating>
            {
                new CompetitorRating
                {
                    Competitor = new Competitor(),
                    RatingCategory = new RatingCategory()
                }
            };

            _ratingRepositoryMock
                .Setup(x => x.GetRatings())
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetRatings();

            // Assert
            result.Should().BeEquivalentTo(expected);

            _ratingRepositoryMock.Verify(
                x => x.GetRatings(),
                Times.Once);
        }

        [Fact]
        public async Task GetGameCompetitorRatings_ReturnsRepositoryResult()
        {
            // Arrange
            const int eventId = 2;

            var expected = new List<DeelnemerRatingDto>
            {
                new DeelnemerRatingDto()
            };

            _ratingRepositoryMock
                .Setup(x => x.GetGameCompetitorRatings(eventId))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetGameCompetitorRatings(eventId);

            // Assert
            result.Should().BeEquivalentTo(expected);

            _ratingRepositoryMock.Verify(
                x => x.GetGameCompetitorRatings(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetRatingsByCompetitorIds_ReturnsRepositoryResult()
        {
            // Arrange
            var competitorIds = new[] { 1, 2, 3 };

            var expected = new List<CompetitorRating>
            {
                new CompetitorRating
                {
                    Competitor = new Competitor(),
                    RatingCategory = new RatingCategory()
                }
            };

            _ratingRepositoryMock
                .Setup(x => x.GetRatingsByCompetitorIds(competitorIds))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetRatingsByCompetitorIds(competitorIds);

            // Assert
            result.Should().BeEquivalentTo(expected);

            _ratingRepositoryMock.Verify(
                x => x.GetRatingsByCompetitorIds(competitorIds),
                Times.Once);
        }
    }
}