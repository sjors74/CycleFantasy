using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class ResultServiceTests
    {
        private readonly Mock<IResultsRepository> _resultsRepositoryMock;
        private readonly Mock<ISpecialResultsRepository> _specialResultsRepositoryMock;
        private readonly Mock<IScoreRepository> _scoreRepositoryMock;
        private readonly ResultService _service;

        public ResultServiceTests()
        {
            _resultsRepositoryMock = new Mock<IResultsRepository>();
            _specialResultsRepositoryMock = new Mock<ISpecialResultsRepository>();
            _scoreRepositoryMock = new Mock<IScoreRepository>();

            _service = new ResultService(
                _resultsRepositoryMock.Object,
                _specialResultsRepositoryMock.Object,
                _scoreRepositoryMock.Object);
        }

        [Fact]
        public async Task GetCompetitorResultsByEventId_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            const int competitorInEventId = 20;

            var expected = new CompetitorScoreDto();

            _resultsRepositoryMock
                .Setup(x => x.GetCompetitorResultsByEventId(eventId, competitorInEventId))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorResultsByEventId(
                eventId,
                competitorInEventId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetCompetitorResultsByEventId(eventId, competitorInEventId),
                Times.Once);
        }

        [Fact]
        public async Task GetResultsByStageId_ReturnsRepositoryResult()
        {
            const int stageId = 5;

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByStageId(stageId))
                .ReturnsAsync(12);

            var result = await _service.GetResultsByStageId(stageId);

            result.Should().Be(12);

            _resultsRepositoryMock.Verify(
                x => x.GetResultsByStageId(stageId),
                Times.Once);
        }

        [Fact]
        public async Task GetEtappeUitslag_ReturnsRepositoryResult()
        {
            const int stageId = 5;
            var expected = new EtappeResultaatDto();

            _resultsRepositoryMock
                .Setup(x => x.GetEtappeUitslag(stageId))
                .ReturnsAsync(expected);

            var result = await _service.GetEtappeUitslag(stageId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetEtappeUitslag(stageId),
                Times.Once);
        }

        [Fact]
        public async Task GetPoolRankingForStage_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            const int stageId = 5;

            var expected = new List<DeelnemerDto>();

            _scoreRepositoryMock
                .Setup(x => x.GetPoolRankingForStage(eventId, stageId))
                .ReturnsAsync(expected);

            var result = await _service.GetPoolRankingForStage(eventId, stageId);

            result.Should().BeSameAs(expected);

            _scoreRepositoryMock.Verify(
                x => x.GetPoolRankingForStage(eventId, stageId),
                Times.Once);
        }

        [Fact]
        public async Task GetScoresByEventIdAsync_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            var expected = new List<DeelnemerStageScore>();

            _scoreRepositoryMock
                .Setup(x => x.GetScoresByEventIdAsync(eventId))
                .ReturnsAsync(expected);

            var result = await _service.GetScoresByEventIdAsync(eventId);

            result.Should().BeSameAs(expected);

            _scoreRepositoryMock.Verify(
                x => x.GetScoresByEventIdAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetStageByIdAsync_ReturnsRepositoryResult()
        {
            const int stageId = 5;
            var expected = new Stage();

            _resultsRepositoryMock
                .Setup(x => x.GetStageByIdAsync(stageId))
                .ReturnsAsync(expected);

            var result = await _service.GetStageByIdAsync(stageId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetStageByIdAsync(stageId),
                Times.Once);
        }

        [Fact]
        public async Task GetResultsByStageAsync_ReturnsRepositoryResult()
        {
            const int stageId = 5;
            var expected = new List<Result>();

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByStageAsync(stageId))
                .ReturnsAsync(expected);

            var result = await _service.GetResultsByStageAsync(stageId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetResultsByStageAsync(stageId),
                Times.Once);
        }

        [Fact]
        public async Task GetSpecialResultsByStageAsync_ReturnsRepositoryResult()
        {
            const int stageId = 5;
            var expected = new List<SpecialResult>();

            _specialResultsRepositoryMock
                .Setup(x => x.GetByStageAsync(stageId))
                .ReturnsAsync(expected);

            var result = await _service.GetSpecialResultsByStageAsync(stageId);

            result.Should().BeSameAs(expected);

            _specialResultsRepositoryMock.Verify(
                x => x.GetByStageAsync(stageId),
                Times.Once);
        }

        [Fact]
        public async Task GetCompetitorsInEventAsync_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            var expected = new List<CompetitorsInEvent>();

            _resultsRepositoryMock
                .Setup(x => x.GetCompetitorsInEventAsync(eventId))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorsInEventAsync(eventId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetCompetitorsInEventAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetConfigurationItemsByConfigAsync_ReturnsRepositoryResult()
        {
            const int configId = 10;
            var expected = new List<ConfigurationItem>();

            _resultsRepositoryMock
                .Setup(x => x.GetConfigurationItemsByConfigAsync(configId))
                .ReturnsAsync(expected);

            var result = await _service.GetConfigurationItemsByConfigAsync(configId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetConfigurationItemsByConfigAsync(configId),
                Times.Once);
        }

        [Fact]
        public async Task GetConfigurationItemSpecialsAsync_ReturnsRepositoryResult()
        {
            const int configId = 10;
            var expected = new List<ConfigurationItemSpecial>();

            _specialResultsRepositoryMock
                .Setup(x => x.GetConfigurationItemsByConfigAsync(configId))
                .ReturnsAsync(expected);

            var result = await _service.GetConfigurationItemSpecialsAsync(configId);

            result.Should().BeSameAs(expected);

            _specialResultsRepositoryMock.Verify(
                x => x.GetConfigurationItemsByConfigAsync(configId),
                Times.Once);
        }

        [Fact]
        public async Task AddResultsAsync_CallsRepository()
        {
            var results = new List<Result>
            {
                new Result(),
                new Result()
            };

            await _service.AddResultsAsync(results);

            _resultsRepositoryMock.Verify(
                x => x.AddResultsAsync(results),
                Times.Once);
        }

        [Fact]
        public async Task AddSpecialResultsAsync_CallsRepository()
        {
            var specialResults = new List<SpecialResult>
            {
                new SpecialResult(),
                new SpecialResult()
            };

            await _service.AddSpecialResultsAsync(specialResults);

            _specialResultsRepositoryMock.Verify(
                x => x.AddResultsAsync(specialResults),
                Times.Once);
        }

        [Fact]
        public async Task GetResultByIdAsync_ReturnsRepositoryResult()
        {
            const int id = 15;
            var expected = new Result();

            _resultsRepositoryMock
                .Setup(x => x.GetResultByIdAsync(id))
                .ReturnsAsync(expected);

            var result = await _service.GetResultByIdAsync(id);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetResultByIdAsync(id),
                Times.Once);
        }

        [Fact]
        public async Task DeleteResultAsync_CallsRepository()
        {
            var result = new Result();

            await _service.DeleteResultAsync(result);

            _resultsRepositoryMock.Verify(
                x => x.DeleteResultAsync(result),
                Times.Once);
        }

        [Fact]
        public async Task ResultExistsAsync_ReturnsRepositoryResult()
        {
            const int id = 15;

            _resultsRepositoryMock
                .Setup(x => x.ResultExistsAsync(id))
                .ReturnsAsync(true);

            var result = await _service.ResultExistsAsync(id);

            result.Should().BeTrue();

            _resultsRepositoryMock.Verify(
                x => x.ResultExistsAsync(id),
                Times.Once);
        }

        [Fact]
        public void GetCompetitorFullName_ReturnsRepositoryResult()
        {
            const int competitorId = 25;

            _resultsRepositoryMock
                .Setup(x => x.GetCompetitorFullName(competitorId))
                .Returns("Test Renner");

            var result = _service.GetCompetitorFullName(competitorId);

            result.Should().Be("Test Renner");

            _resultsRepositoryMock.Verify(
                x => x.GetCompetitorFullName(competitorId),
                Times.Once);
        }

        [Fact]
        public async Task RecalculateEventScoresAsync_CallsRepository()
        {
            const int eventId = 10;

            await _service.RecalculateEventScoresAsync(eventId);

            _resultsRepositoryMock.Verify(
                x => x.RecalculateEventScoresAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetTotalScoresByEventIdAsync_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            var expected = new List<DeelnemerScore>();

            _resultsRepositoryMock
                .Setup(x => x.GetTotalScoresByEventIdAsync(eventId))
                .ReturnsAsync(expected);

            var result = await _service.GetTotalScoresByEventIdAsync(eventId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetTotalScoresByEventIdAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetPickDetailsAsync_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            const int competitorInEventId = 20;
            var expected = new List<PickDetailDto>();

            _resultsRepositoryMock
                .Setup(x => x.GetPickDetailsAsync(eventId, competitorInEventId))
                .ReturnsAsync(expected);

            var result = await _service.GetPickDetailsAsync(
                eventId,
                competitorInEventId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetPickDetailsAsync(eventId, competitorInEventId),
                Times.Once);
        }

        [Fact]
        public async Task GetCompetitorResultsForEvent_ReturnsRepositoryResult()
        {
            const int eventId = 10;
            var expected = new List<CompetitorScoreDto>();

            _resultsRepositoryMock
                .Setup(x => x.GetCompetitorResultsForEvent(eventId))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorResultsForEvent(eventId);

            result.Should().BeSameAs(expected);

            _resultsRepositoryMock.Verify(
                x => x.GetCompetitorResultsForEvent(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetSpecialResultByIdAsync_ReturnsRepositoryResult()
        {
            const int id = 15;
            var expected = new SpecialResult();

            _specialResultsRepositoryMock
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(expected);

            var result = await _service.GetSpecialResultByIdAsync(id);

            result.Should().BeSameAs(expected);

            _specialResultsRepositoryMock.Verify(
                x => x.GetByIdAsync(id),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSpecialResultAsync_CallsRepository()
        {
            const int id = 15;

            await _service.DeleteSpecialResultAsync(id);

            _specialResultsRepositoryMock.Verify(
                x => x.DeleteAsync(id),
                Times.Once);
        }

        [Fact]
        public async Task SyncResultsAsync_CallsRepository()
        {
            const int stageId = 5;

            var results = new List<Result>
            {
                new Result()
            };

            var specialResults = new List<SpecialResult>
            {
                new SpecialResult()
            };

            await _service.SyncResultsAsync(
                stageId,
                results,
                specialResults);

            _resultsRepositoryMock.Verify(
                x => x.SyncResultsAsync(
                    stageId,
                    results,
                    specialResults),
                Times.Once);
        }

        [Fact]
        public async Task GetScoreBreakdownByEventIdAsync_MapsRepositoryResults()
        {
            const int eventId = 10;

            var scores = new List<DeelnemerScoreBreakdown>
            {
                new DeelnemerScoreBreakdown
                {
                    GameCompetitorEventId = 1,
                    NormalPoints = 25,
                    SpecialPoints = 10
                },
                new DeelnemerScoreBreakdown
                {
                    GameCompetitorEventId = 2,
                    NormalPoints = 15,
                    SpecialPoints = 5
                }
            };

            _resultsRepositoryMock
                .Setup(x => x.GetScoreBreakdownByEventIdAsync(eventId))
                .ReturnsAsync(scores);

            var result = await _service.GetScoreBreakdownByEventIdAsync(eventId);

            result.Should().HaveCount(2);

            result[0].GameCompetitorEventId.Should().Be(1);
            result[0].NormalScore.Should().Be(25);
            result[0].SpecialScore.Should().Be(10);

            result[1].GameCompetitorEventId.Should().Be(2);
            result[1].NormalScore.Should().Be(15);
            result[1].SpecialScore.Should().Be(5);

            _resultsRepositoryMock.Verify(
                x => x.GetScoreBreakdownByEventIdAsync(eventId),
                Times.Once);
        }

        [Fact]
        public async Task GetResultsByEventId_GroupsResultsAndCalculatesPoints()
        {
            // Arrange
            const int eventId = 10;
            const int competitorInEventId = 100;

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Tadej",
                LastName = "Pogacar"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Name = "UAE Team Emirates"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = competitorInEventId,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventId = eventId
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = eventId
            };

            var results = new List<Result>
            {
                new Result
                {
                    Id = 1,
                    StageId = stage.Id,
                    Stage = stage,
                    CompetitorInEventId = competitorInEventId,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItem = new ConfigurationItem
                    {
                        Id = 1,
                        Score = 10
                    }
                },
                new Result
                {
                    Id = 2,
                    StageId = stage.Id,
                    Stage = stage,
                    CompetitorInEventId = competitorInEventId,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItem = new ConfigurationItem
                    {
                        Id = 2,
                        Score = 5
                    }
                }
            };

            var specialResults = new List<SpecialResult>
            {
                new SpecialResult
                {
                    Id = 1,
                    CompetitorInEventId = competitorInEventId,
                    Special = new ConfigurationItemSpecial
                    {
                        Id = 1,
                        Score = 7
                    }
                }
            };

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByEventId(eventId))
                .ReturnsAsync(results);

            _specialResultsRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(specialResults);

            // Act
            var result = (await _service.GetResultsByEventId(eventId)).ToList();

            // Assert
            result.Should().HaveCount(1);

            var ranking = result[0];

            ranking.CompetitorName.Should().Be("Tadej Pogacar");
            ranking.CompetitorTeam.Should().Be("UAE Team Emirates");
            ranking.EventId.Should().Be(eventId);
            ranking.CompetitorInEventId.Should().Be(competitorInEventId);
            ranking.NormalPoints.Should().Be(15);
            ranking.SpecialPoints.Should().Be(7);
            ranking.TotalPoints.Should().Be(22);
            ranking.Position.Should().Be(1);
        }

        [Fact]
        public async Task GetResultsByEventId_WithoutSpecialResults_ReturnsZeroSpecialPoints()
        {
            // Arrange
            const int eventId = 10;
            const int competitorInEventId = 100;

            var competitor = new Competitor
            {
                CompetitorId = 1,
                FirstName = "Jonas",
                LastName = "Vingegaard"
            };

            var teamYear = new TeamYear
            {
                TeamYearId = 1,
                Name = "Visma | Lease a Bike"
            };

            var competitorInTeam = new CompetitorInTeam
            {
                Id = 1,
                CompetitorId = competitor.CompetitorId,
                Competitor = competitor,
                TeamYearId = teamYear.TeamYearId,
                TeamYear = teamYear
            };

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = competitorInEventId,
                CompetitorInTeamId = competitorInTeam.Id,
                CompetitorInTeam = competitorInTeam,
                EventId = eventId
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = eventId
            };

            var results = new List<Result>
            {
                new Result
                {
                    Id = 1,
                    StageId = stage.Id,
                    Stage = stage,
                    CompetitorInEventId = competitorInEventId,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItem = new ConfigurationItem
                    {
                        Id = 1,
                        Score = 12
                    }
                }
            };

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByEventId(eventId))
                .ReturnsAsync(results);

            _specialResultsRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(new List<SpecialResult>());

            // Act
            var result = (await _service.GetResultsByEventId(eventId)).ToList();

            // Assert
            result.Should().ContainSingle();

            result[0].CompetitorName.Should().Be("Jonas Vingegaard");
            result[0].NormalPoints.Should().Be(12);
            result[0].SpecialPoints.Should().Be(0);
            result[0].TotalPoints.Should().Be(12);
            result[0].Position.Should().Be(1);
        }

        [Fact]
        public async Task GetResultsByEventId_WithMissingCompetitorData_UsesUnknownValues()
        {
            // Arrange
            const int eventId = 10;
            const int competitorInEventId = 100;

            var competitorInEvent = new CompetitorsInEvent
            {
                Id = competitorInEventId,
                EventId = eventId,
                CompetitorInTeam = new CompetitorInTeam
                {
                    Id = 1,
                    Competitor = null!,
                    TeamYear = null!
                }
            };

            var stage = new Stage
            {
                Id = 1,
                EventId = eventId
            };

            var results = new List<Result>
            {
                new Result
                {
                    Id = 1,
                    StageId = stage.Id,
                    Stage = stage,
                    CompetitorInEventId = competitorInEventId,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItem = new ConfigurationItem
                    {
                        Id = 1,
                        Score = 10
                    }
                }
            };

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByEventId(eventId))
                .ReturnsAsync(results);

            _specialResultsRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(new List<SpecialResult>());

            // Act
            var result = (await _service.GetResultsByEventId(eventId)).ToList();

            // Assert
            result.Should().ContainSingle();

            result[0].CompetitorName.Should().Be("onbekend");
            result[0].CompetitorTeam.Should().Be("onbekend");
            result[0].NormalPoints.Should().Be(10);
            result[0].SpecialPoints.Should().Be(0);
        }

        [Fact]
        public async Task GetResultsByEventId_WithEqualScores_AssignsSamePosition()
        {
            // Arrange
            const int eventId = 10;

            var stage = new Stage
            {
                Id = 1,
                EventId = eventId
            };

            var results = new List<Result>();

            for (int i = 1; i <= 3; i++)
            {
                var competitor = new Competitor
                {
                    CompetitorId = i,
                    FirstName = $"Rijder{i}",
                    LastName = "Test"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = i,
                    Name = $"Team {i}"
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = i,
                    CompetitorId = i,
                    Competitor = competitor,
                    TeamYearId = i,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = i,
                    CompetitorInTeamId = i,
                    CompetitorInTeam = competitorInTeam,
                    EventId = eventId
                };

                results.Add(new Result
                {
                    Id = i,
                    StageId = stage.Id,
                    Stage = stage,
                    CompetitorInEventId = i,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItem = new ConfigurationItem
                    {
                        Id = i,
                        Score = i <= 2 ? 20 : 15
                    }
                });
            }

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByEventId(eventId))
                .ReturnsAsync(results);

            _specialResultsRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(new List<SpecialResult>());

            // Act
            var result = (await _service.GetResultsByEventId(eventId)).ToList();

            // Assert
            result.Should().HaveCount(3);

            result[0].TotalPoints.Should().Be(20);
            result[0].Position.Should().Be(1);

            result[1].TotalPoints.Should().Be(20);
            result[1].Position.Should().Be(1);

            result[2].TotalPoints.Should().Be(15);
            result[2].Position.Should().Be(3);
        }

        [Fact]
        public async Task GetResultsByEventId_OnlyTop15_ReturnsTop15()
        {
            // Arrange
            const int eventId = 10;

            var stage = new Stage
            {
                Id = 1,
                EventId = eventId
            };

            var results = new List<Result>();

            for (int i = 1; i <= 16; i++)
            {
                var competitor = new Competitor
                {
                    CompetitorId = i,
                    FirstName = $"Rijder{i}",
                    LastName = "Test"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = i,
                    Name = $"Team {i}"
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = i,
                    CompetitorId = i,
                    Competitor = competitor,
                    TeamYearId = i,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = i,
                    CompetitorInTeamId = i,
                    CompetitorInTeam = competitorInTeam,
                    EventId = eventId
                };

                results.Add(new Result
                {
                    Id = i,
                    StageId = stage.Id,
                    Stage = stage,
                    CompetitorInEventId = i,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItem = new ConfigurationItem
                    {
                        Id = i,
                        Score = 100 - i
                    }
                });
            }

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByEventId(eventId))
                .ReturnsAsync(results);

            _specialResultsRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(new List<SpecialResult>());

            // Act
            var result = (await _service.GetResultsByEventId(eventId, onlyTop15: true))
                .ToList();

            // Assert
            result.Should().HaveCount(15);
            result.Should().OnlyContain(x => x.TotalPoints >= 85);
            result[0].TotalPoints.Should().Be(99);
            result[14].TotalPoints.Should().Be(85);
        }

        [Fact]
        public async Task GetResultsByEventId_OnlyTop15_IncludesTiesAt15thPlace()
        {
            // Arrange
            const int eventId = 10;

            var stage = new Stage
            {
                Id = 1,
                EventId = eventId
            };

            var results = new List<Result>();

            for (int i = 1; i <= 16; i++)
            {
                var competitor = new Competitor
                {
                    CompetitorId = i,
                    FirstName = $"Rijder{i}",
                    LastName = "Test"
                };

                var teamYear = new TeamYear
                {
                    TeamYearId = i,
                    Name = $"Team {i}"
                };

                var competitorInTeam = new CompetitorInTeam
                {
                    Id = i,
                    CompetitorId = i,
                    Competitor = competitor,
                    TeamYearId = i,
                    TeamYear = teamYear
                };

                var competitorInEvent = new CompetitorsInEvent
                {
                    Id = i,
                    CompetitorInTeamId = i,
                    CompetitorInTeam = competitorInTeam,
                    EventId = eventId
                };

                results.Add(new Result
                {
                    Id = i,
                    StageId = stage.Id,
                    Stage = stage,
                    CompetitorInEventId = i,
                    CompetitorInEvent = competitorInEvent,
                    ConfigurationItem = new ConfigurationItem
                    {
                        Id = i,
                        Score = i <= 14 ? 100 - i : 85
                    }
                });
            }

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByEventId(eventId))
                .ReturnsAsync(results);

            _specialResultsRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(new List<SpecialResult>());

            // Act
            var result = (await _service.GetResultsByEventId(eventId, onlyTop15: true))
                .ToList();

            // Assert
            result.Should().HaveCount(16);

            result[14].TotalPoints.Should().Be(85);
            result[15].TotalPoints.Should().Be(85);

            result[14].Position.Should().Be(15);
            result[15].Position.Should().Be(15);
        }

        [Fact]
        public async Task GetResultsByEventId_OnlyTop15_WithNoResults_ReturnsEmptyList()
        {
            // Arrange
            const int eventId = 10;

            _resultsRepositoryMock
                .Setup(x => x.GetResultsByEventId(eventId))
                .ReturnsAsync(new List<Result>());

            _specialResultsRepositoryMock
                .Setup(x => x.GetByEventId(eventId))
                .ReturnsAsync(new List<SpecialResult>());

            // Act
            var result = (await _service.GetResultsByEventId(
                eventId,
                onlyTop15: true)).ToList();

            // Assert
            result.Should().BeEmpty();
        }
    }
}