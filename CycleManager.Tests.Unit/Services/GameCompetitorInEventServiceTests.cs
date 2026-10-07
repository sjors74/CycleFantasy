using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class GameCompetitorInEventServiceTests
    {
        private readonly Mock<IGameCompetitorInEventRepository> _repoMock;
        private readonly Mock<IGameCompetitorEventPickRepository> _pickRepositoryMock;
        private readonly Mock<ICompetitorsInEventRepository> _competitorRepoMock;
        private readonly GameCompetitorInEventService _service;

        public GameCompetitorInEventServiceTests()
        {
            _repoMock = new Mock<IGameCompetitorInEventRepository>();
            _pickRepositoryMock = new Mock<IGameCompetitorEventPickRepository>();
            _competitorRepoMock = new Mock<ICompetitorsInEventRepository>();

            _service = new GameCompetitorInEventService(
                _repoMock.Object,
                _pickRepositoryMock.Object,
                _competitorRepoMock.Object);
        }

        [Fact]
        public async Task Create_AddsEntityAndSaves()
        {
            var entity = new GameCompetitorEvent
            {
                Id = 1,
                TeamName = "Mijn Team"
            };

            await _service.Create(entity);

            _repoMock.Verify(x => x.Add(entity), Times.Once);
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesEntityAndSaves()
        {
            var entity = new GameCompetitorEvent
            {
                Id = 1,
                TeamName = "Mijn Team"
            };

            await _service.Delete(entity);

            _repoMock.Verify(x => x.Remove(entity), Times.Once);
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllCompetitorsInEvent_ReturnsRepositoryResult()
        {
            var expected = new List<GameCompetitorEvent>
            {
                new() { Id = 1, TeamName = "Team 1" },
                new() { Id = 2, TeamName = "Team 2" }
            };

            _repoMock
                .Setup(x => x.GetAllGameCompetitorsInEventByEventId(10))
                .ReturnsAsync(expected);

            var result = await _service.GetAllCompetitorsInEvent(10);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetGameCompetitorEventById_ReturnsRepositoryResult()
        {
            var expected = new GameCompetitorEvent
            {
                Id = 10,
                TeamName = "Team 1"
            };

            _repoMock
                .Setup(x => x.GetGameCompetitorInEventById(10))
                .ReturnsAsync(expected);

            var result = await _service.GetGameCompetitorEventById(10);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public void GetPicks_ReturnsRepositoryResult()
        {
            var expected = new List<GameCompetitorEventPick>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            }.AsQueryable();

            _pickRepositoryMock
                .Setup(x => x.GetCompetitorEventPicksByEventId(10))
                .Returns(expected);

            var result = _service.GetPicks(10);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetAllPicks_ReturnsRepositoryResult()
        {
            var expected = new List<GameCompetitorEventPick>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            _pickRepositoryMock
                .Setup(x => x.GetCompetitorEventPicksById(10))
                .ReturnsAsync(expected);

            var result = await _service.GetAllPicks(10);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetNumberOfPicks_ReturnsNumberOfPicks()
        {
            var picks = new List<GameCompetitorEventPick>
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            };

            _pickRepositoryMock
                .Setup(x => x.GetCompetitorEventPicksById(20))
                .ReturnsAsync(picks);

            var result = await _service.GetNumberOfPicks(10, 20);

            result.Should().Be(3);
        }

        [Fact]
        public async Task UpdateAsync_WhenEntityDoesNotExist_ThrowsException()
        {
            var dto = new DeelnemerEditDto
            {
                Id = 123,
                TeamName = "Nieuw Team",
                UserId = "user-1"
            };

            _repoMock
                .Setup(x => x.GetById(123))
                .ReturnsAsync((GameCompetitorEvent?)null);

            var act = () => _service.UpdateAsync(dto);

            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Deelnemer niet gevonden.");
        }

        [Fact]
        public async Task UpdateAsync_WhenEntityExists_UpdatesEntityAndSaves()
        {
            var entity = new GameCompetitorEvent
            {
                Id = 123,
                TeamName = "Oude Naam",
                UserId = "old-user"
            };

            var dto = new DeelnemerEditDto
            {
                Id = 123,
                TeamName = "Nieuwe Naam",
                UserId = "new-user"
            };

            _repoMock
                .Setup(x => x.GetById(123))
                .ReturnsAsync(entity);

            await _service.UpdateAsync(dto);

            entity.TeamName.Should().Be("Nieuwe Naam");
            entity.UserId.Should().Be("new-user");

            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCompetitors_ReturnsRepositoryResult()
        {
            var expected = new List<CompetitorsInEvent>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            _competitorRepoMock
                .Setup(x => x.GetRandomNumberofCompetitors(10, 5))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitors(10, 5);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetAllPicksAsCompetitorIds_ReturnsCompetitorIds()
        {
            var picks = new List<GameCompetitorEventPick>
            {
                new()
                {
                    Id = 1,
                    CompetitorsInEvent = new CompetitorsInEvent
                    {
                        Id = 10,
                        CompetitorInTeamId = 101
                    }
                },
                new()
                {
                    Id = 2,
                    CompetitorsInEvent = new CompetitorsInEvent
                    {
                        Id = 20,
                        CompetitorInTeamId = 202
                    }
                }
            };

            _pickRepositoryMock
                .Setup(x => x.GetCompetitorEventPicksById(10))
                .ReturnsAsync(picks);

            var result = await _service.GetAllPicksAsCompetitorIds(10);

            result.Should().BeEquivalentTo(new[] { 101, 202 });
        }

        [Fact]
        public async Task GetCompetitorInEventById_ReturnsRepositoryResult()
        {
            var expected = new CompetitorsInEvent
            {
                Id = 123,
                CompetitorInTeamId = 456
            };

            _competitorRepoMock
                .Setup(x => x.GetById(123))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorInEventById(123);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task CreateGameCompetitorEventAsync_ReturnsRepositoryResult()
        {
            var dto = new DeelnemerCreateDto
            {
                TeamName = "Mijn Team",
                UserId = "user-1",
                EventId = 10
            };

            var expected = new GameCompetitorEvent
            {
                Id = 123,
                TeamName = "Mijn Team",
                UserId = "user-1",
                EventId = 10
            };

            _repoMock
                .Setup(x => x.CreateGameCompetitorEventAsync(dto))
                .ReturnsAsync(expected);

            var result = await _service.CreateGameCompetitorEventAsync(dto);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task RemovePickFromEvent_RemovesPickAndSaves()
        {
            await _service.RemovePickFromEvent(123);

            _pickRepositoryMock.Verify(
                x => x.RemovePickFromEvent(123),
                Times.Once);

            _pickRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task AddPicks_AddsPicksAndSaves()
        {
            var picks = new List<GameCompetitorEventPick>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            await _service.AddPicks(picks);

            _pickRepositoryMock.Verify(
                x => x.AddRange(picks),
                Times.Once);

            _pickRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task DeleteGameCompetitorEventAsync_DeletesAndSaves()
        {
            await _service.DeleteGameCompetitorEventAsync(123);

            _pickRepositoryMock.Verify(
                x => x.DeleteGameCompetitorEventAsync(123),
                Times.Once);

            _pickRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetDropdownListAsync_ReturnsGroupedCompetitors()
        {
            var competitors = new List<CompetitorsInEvent>
            {
                new()
                {
                    Id = 1,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Id = 101,
                        Competitor = new Competitor
                        {
                            CompetitorId = 1001,
                            FirstName = "Tadej",
                            LastName = "Pogacar"
                        },
                        TeamYear = new TeamYear
                        {
                            Name = "UAE Team"
                        }
                    }
                },
                new()
                {
                    Id = 2,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Id = 102,
                        Competitor = new Competitor
                        {
                            CompetitorId = 1002,
                            FirstName = "Jonas",
                            LastName = "Vingegaard"
                        },
                        TeamYear = new TeamYear
                        {
                            Name = "Visma"
                        }
                    }
                }
            };

            _competitorRepoMock
                .Setup(x => x.GetCompetitorsInEventList(10))
                .ReturnsAsync(competitors);

            var result = (await _service.GetDropdownListAsync(10)).ToList();

            result.Should().HaveCount(2);

            result[0].Value.Should().Be("1");
            result[0].Text.Should().Be("Tadej Pogacar");
            result[0].Group.Should().NotBeNull();
            result[0].Group!.Name.Should().Be("UAE Team");

            result[1].Value.Should().Be("2");
            result[1].Text.Should().Be("Jonas Vingegaard");
            result[1].Group.Should().NotBeNull();
            result[1].Group!.Name.Should().Be("Visma");
        }

        [Fact]
        public async Task GetDropdownListAsync_WhenTeamYearIsMissing_UsesUnknownGroup()
        {
            var competitors = new List<CompetitorsInEvent>
            {
                new()
                {
                    Id = 1,
                    CompetitorInTeam = new CompetitorInTeam
                    {
                        Id = 101,
                        Competitor = new Competitor
                        {
                            CompetitorId = 1001,
                            FirstName = "Tadej",
                            LastName = "Pogacar"
                        },
                        TeamYear = null!
                    }
                }
            };

            _competitorRepoMock
                .Setup(x => x.GetCompetitorsInEventList(10))
                .ReturnsAsync(competitors);

            var result = (await _service.GetDropdownListAsync(10)).ToList();

            result.Should().HaveCount(1);
            result[0].Value.Should().Be("1");
            result[0].Text.Should().Be("Tadej Pogacar");
            result[0].Group.Should().NotBeNull();
            result[0].Group!.Name.Should().Be("onbekend");
        }

        [Fact]
        public async Task RenamePoolAsync_WhenNewNameIsEmpty_ReturnsFalse()
        {
            var dto = new RenamePoolDto
            {
                DeelnemerId = 123,
                UserId = "user-1",
                NieuweNaam = "   "
            };

            var result = await _service.RenamePoolAsync(dto);

            result.Should().BeFalse();

            _repoMock.Verify(
                x => x.RenamePoolAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RenamePoolAsync_WhenNewNameIsValid_ReturnsRepositoryResult()
        {
            var dto = new RenamePoolDto
            {
                DeelnemerId = 123,
                UserId = "user-1",
                NieuweNaam = "Nieuwe Pool"
            };

            _repoMock
                .Setup(x => x.RenamePoolAsync(123, "user-1", "Nieuwe Pool"))
                .ReturnsAsync(true);

            var result = await _service.RenamePoolAsync(dto);

            result.Should().BeTrue();

            _repoMock.Verify(
                x => x.RenamePoolAsync(123, "user-1", "Nieuwe Pool"),
                Times.Once);
        }
    }
}