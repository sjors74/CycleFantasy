using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class CompetitorInEventServiceTests
    {
        private readonly Mock<ICompetitorsInEventRepository> _repositoryMock;
        private readonly CompetitorInEventService _service;

        public CompetitorInEventServiceTests()
        {
            _repositoryMock = new Mock<ICompetitorsInEventRepository>();
            _service = new CompetitorInEventService(_repositoryMock.Object);
        }

        [Fact]
        public async Task Create_AddsRangeAndSaves()
        {
            var entities = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent(),
                new CompetitorsInEvent()
            };

            await _service.Create(entities);

            _repositoryMock.Verify(
                x => x.AddRange(entities),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetCompetitorById_ReturnsRepositoryResult()
        {
            const int id = 10;
            var expected = new CompetitorsInEvent();

            _repositoryMock
                .Setup(x => x.GetByCompetitorId(id))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorById(id);

            result.Should().BeSameAs(expected);

            _repositoryMock.Verify(
                x => x.GetByCompetitorId(id),
                Times.Once);
        }

        [Fact]
        public async Task GetCompetitors_ReturnsRepositoryResult()
        {
            const int eventId = 10;

            var expected = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent(),
                new CompetitorsInEvent()
            };

            _repositoryMock
                .Setup(x => x.GetCompetitors(eventId))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitors(eventId);

            result.Should().BeSameAs(expected);

            _repositoryMock.Verify(
                x => x.GetCompetitors(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetCompetitorsInEventByIds_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            const int competitorId = 20;

            var expected = new CompetitorsInEvent();

            _repositoryMock
                .Setup(x => x.GetCompetitorsInEventByIds(eventId, competitorId))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorsInEventByIds(
                eventId,
                competitorId);

            result.Should().BeSameAs(expected);

            _repositoryMock.Verify(
                x => x.GetCompetitorsInEventByIds(eventId, competitorId),
                Times.Once);
        }

        [Fact]
        public async Task Update_UpdatesAndSaves()
        {
            var entity = new CompetitorsInEvent();

            await _service.Update(entity);

            _repositoryMock.Verify(
                x => x.Update(entity),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task FindOrCreate_WhenCompetitorExists_ReturnsExisting()
        {
            const int eventId = 10;
            const int competitorId = 20;

            var existing = new CompetitorsInEvent
            {
                Id = 100,
                EventId = eventId,
                CompetitorInTeamId = competitorId
            };

            _repositoryMock
                .Setup(x => x.GetCompetitorsInEventByIds(eventId, competitorId))
                .ReturnsAsync(existing);

            var result = await _service.FindOrCreate(eventId, competitorId);

            result.Should().BeSameAs(existing);

            _repositoryMock.Verify(
                x => x.GetCompetitorsInEventByIds(eventId, competitorId),
                Times.Once);

            _repositoryMock.Verify(
                x => x.Add(It.IsAny<CompetitorsInEvent>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task FindOrCreate_WhenCompetitorDoesNotExist_CreatesAndSaves()
        {
            const int eventId = 10;
            const int competitorId = 20;

            _repositoryMock
                .Setup(x => x.GetCompetitorsInEventByIds(eventId, competitorId))
                .ReturnsAsync((CompetitorsInEvent?)null);

            var result = await _service.FindOrCreate(eventId, competitorId);

            result.Should().NotBeNull();
            result.EventId.Should().Be(eventId);
            result.CompetitorInTeamId.Should().Be(competitorId);

            _repositoryMock.Verify(
                x => x.Add(It.Is<CompetitorsInEvent>(x =>
                    x.EventId == eventId &&
                    x.CompetitorInTeamId == competitorId)),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetRandomNumberofCompetitors_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            const int number = 5;

            var expected = new List<CompetitorsInEvent>
            {
                new CompetitorsInEvent(),
                new CompetitorsInEvent()
            };

            _repositoryMock
                .Setup(x => x.GetRandomNumberofCompetitors(eventId, number))
                .ReturnsAsync(expected);

            var result = await _service.GetRandomNumberofCompetitors(
                eventId,
                number);

            result.Should().BeSameAs(expected);

            _repositoryMock.Verify(
                x => x.GetRandomNumberofCompetitors(eventId, number),
                Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesAndSaves()
        {
            var entity = new CompetitorsInEvent();

            await _service.Delete(entity);

            _repositoryMock.Verify(
                x => x.Remove(entity),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}