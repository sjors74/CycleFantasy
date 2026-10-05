using CycleManager.Domain.Dto;
using CycleManager.Domain.Interfaces;
using CycleManager.Domain.Models;
using CycleManager.Services;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class CompetitorServiceTests
    {
        private readonly Mock<ICompetitorRepository> _competitorRepositoryMock;
        private readonly Mock<ICompetitorInTeamRepository> _competitorInTeamRepositoryMock;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly Mock<ICountryRepository> _countryRepositoryMock;
        private readonly Mock<ISeasonYearRepository> _seasonYearRepositoryMock;
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;

        private readonly CompetitorService _service;

        public CompetitorServiceTests()
        {
            _competitorRepositoryMock = new Mock<ICompetitorRepository>();
            _competitorInTeamRepositoryMock = new Mock<ICompetitorInTeamRepository>();
            _teamRepositoryMock = new Mock<ITeamRepository>();
            _countryRepositoryMock = new Mock<ICountryRepository>();
            _seasonYearRepositoryMock = new Mock<ISeasonYearRepository>();
            _ratingRepositoryMock = new Mock<IRatingRepository>();

            _service = new CompetitorService(
                _competitorRepositoryMock.Object,
                _competitorInTeamRepositoryMock.Object,
                _teamRepositoryMock.Object,
                _countryRepositoryMock.Object,
                _seasonYearRepositoryMock.Object,
                _ratingRepositoryMock.Object);
        }

        [Fact]
        public async Task UpdateCompetitorWithTeam_WhenCompetitorNotFound_ThrowsException()
        {
            var dto = new CompetitorEditDto
            {
                CompetitorId = 123
            };

            _competitorRepositoryMock
                .Setup(x => x.GetByCompetitorId(123))
                .ReturnsAsync((Competitor?)null);

            var act = () => _service.UpdateCompetitorWithTeam(dto);

            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Competitor not found");

            _competitorRepositoryMock.Verify(
                x => x.UpdateCompetitorAsync(It.IsAny<Competitor>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateCompetitorWithTeam_UpdatesCompetitorAndExistingTeam()
        {
            var competitor = new Competitor
            {
                CompetitorId = 123,
                FirstName = "Old",
                LastName = "Name",
                PcsName = "Old Pcs",
                PcsScraperName = "Old Scraper",
                CyclingFlashScraperName = "Old Flash",
                CountryId = 1,
                CompetitorInTeams =
                [
                    new CompetitorInTeam
                    {
                        Id = 10,
                        IsNationalChampion = false
                    }
                ]
            };

            var scrapedDate = new DateTime(2026, 10, 1);

            var dto = new CompetitorEditDto
            {
                CompetitorId = 123,
                FirstName = "New",
                LastName = "Name",
                PcsName = null,
                PcsScraperName = null,
                CyclingFlashScraperName = null,
                CyclingFlahsLastScraped = scrapedDate,
                CountryId = 2,
                CompetitorInTeams =
                [
                    new CompetitorInTeamDto
                    {
                        CompetitorInTeamId = 10,
                        IsNationalChampion = true
                    }
                ]
            };

            _competitorRepositoryMock
                .Setup(x => x.GetByCompetitorId(123))
                .ReturnsAsync(competitor);

            await _service.UpdateCompetitorWithTeam(dto);

            competitor.FirstName.Should().Be("New");
            competitor.LastName.Should().Be("Name");
            competitor.PcsName.Should().Be("");
            competitor.PcsScraperName.Should().Be("");
            competitor.CyclingFlashScraperName.Should().Be("");
            competitor.CyclingFlashLastScraped.Should().Be(scrapedDate);
            competitor.CountryId.Should().Be(2);

            competitor.CompetitorInTeams.Single().IsNationalChampion.Should().BeTrue();

            _competitorRepositoryMock.Verify(
                x => x.UpdateCompetitorAsync(competitor),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCompetitorWithTeam_WhenTeamDoesNotExist_DoesNotUpdateTeam()
        {
            var competitor = new Competitor
            {
                CompetitorId = 123,
                FirstName = "Test",
                LastName = "Rider",
                CompetitorInTeams =
                [
                    new CompetitorInTeam
                    {
                        Id = 10,
                        IsNationalChampion = false
                    }
                ]
            };

            var dto = new CompetitorEditDto
            {
                CompetitorId = 123,
                FirstName = "Test",
                LastName = "Rider",
                CountryId = 1,
                CompetitorInTeams =
                [
                    new CompetitorInTeamDto
                    {
                        CompetitorInTeamId = 999,
                        IsNationalChampion = true
                    }
                ]
            };

            _competitorRepositoryMock
                .Setup(x => x.GetByCompetitorId(123))
                .ReturnsAsync(competitor);

            await _service.UpdateCompetitorWithTeam(dto);

            competitor.CompetitorInTeams.Single().IsNationalChampion
                .Should().BeFalse();

            _competitorRepositoryMock.Verify(
                x => x.UpdateCompetitorAsync(competitor),
                Times.Once);
        }

        [Fact]
        public async Task GetCompetitorForEdit_WhenCompetitorNotFound_ReturnsNull()
        {
            const int competitorId = 123;

            _competitorRepositoryMock
                .Setup(x => x.GetByCompetitorId(competitorId))
                .ReturnsAsync((Competitor?)null);

            var result = await _service.GetCompetitorForEdit(competitorId);

            result.Should().BeNull();

            _seasonYearRepositoryMock.Verify(
                x => x.GetAllAsync(),
                Times.Never);

            _countryRepositoryMock.Verify(
                x => x.GetAll(),
                Times.Never);

            _ratingRepositoryMock.Verify(
                x => x.GetRatingCategories(),
                Times.Never);

            _ratingRepositoryMock.Verify(
                x => x.GetRatingsByCompetitorId(competitorId),
                Times.Never);
        }

        [Fact]
        public async Task GetCompetitorForEdit_WhenCompetitorExists_ReturnsMappedDto()
        {
            const int competitorId = 123;

            var competitor = new Competitor
            {
                CompetitorId = competitorId,
                FirstName = "Tadej",
                LastName = "Pogacar",
                PcsName = "Tadej Pogacar",
                PcsScraperName = "tadej-pogacar",
                CyclingFlashScraperName = "tadej-pogacar",
                CyclingFlashLastScraped = new DateTime(2026, 9, 1),
                CountryId = 10,
                CompetitorInTeams =
                [
                    new CompetitorInTeam
                    {
                        Id = 100,
                        TeamYearId = 200,
                        IsNationalChampion = true,
                        TeamYear = new TeamYear
                        {
                            SeasonYearId = 2,
                            Year = 2026,
                            Team = new Team
                            {
                                CurrentTeamName = "UAE Team Emirates"
                            }
                        }
                    }
                ]
            };

            var countries = new List<Country>
            {
                new Country
                {
                    CountryId = 10,
                    CountryNameLong = "Nederland",
                    CountryNameShort = "NL"
                }
            };

            var seasonYears = new List<SeasonYear>
            {
                new SeasonYear
                {
                    SeasonYearId = 1,
                    Year = 2025,
                    Active = false
                },
                new SeasonYear
                {
                    SeasonYearId = 2,
                    Year = 2026,
                    Active = true
                }
            };

            var categories = new List<RatingCategory>
            {
                new RatingCategory
                {
                    RatingCategoryId = 2,
                    Name = "Sprint",
                    IsActive = true,
                    Color = "#00FF00",
                    DisplayOrder = 2
                },
                new RatingCategory
                {
                    RatingCategoryId = 1,
                    Name = "GC",
                    IsActive = true,
                    Color = "#FF0000",
                    DisplayOrder = 1
                }
            };

            var ratings = new List<CompetitorRating>
            {
                new CompetitorRating
                {
                    Competitor = competitor,
                    RatingCategoryId = 2,
                    Rating = 6,
                    RatingCategory = new RatingCategory
                    {
                        RatingCategoryId = 2,
                        Name = "Sprint",
                        Code = "SPR"
                    }
                }
            };

            _competitorRepositoryMock
                .Setup(x => x.GetByCompetitorId(competitorId))
                .ReturnsAsync(competitor);

            _seasonYearRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(seasonYears);

            _countryRepositoryMock
                .Setup(x => x.GetAll())
                .ReturnsAsync(countries);

            _ratingRepositoryMock
                .Setup(x => x.GetRatingCategories())
                .ReturnsAsync(categories);

            _ratingRepositoryMock
                .Setup(x => x.GetRatingsByCompetitorId(competitorId))
                .ReturnsAsync(ratings);

            var result = await _service.GetCompetitorForEdit(competitorId);

            result.Should().NotBeNull();

            result!.CompetitorId.Should().Be(competitorId);
            result.FirstName.Should().Be("Tadej");
            result.LastName.Should().Be("Pogacar");
            result.PcsName.Should().Be("Tadej Pogacar");
            result.PcsScraperName.Should().Be("tadej-pogacar");
            result.CyclingFlashScraperName.Should().Be("tadej-pogacar");
            result.CyclingFlahsLastScraped.Should()
                .Be(new DateTime(2026, 9, 1));
            result.CountryId.Should().Be(10);

            result.Countries.Should().ContainSingle();
            result.Countries.First().Id.Should().Be(10);
            result.Countries.First().CountryNameLong.Should().Be("Nederland");
            result.Countries.First().CountryNameShort.Should().Be("NL");

            result.AvailableYears.Should().HaveCount(2);
            result.AvailableYears.First().Year.Should().Be(2026);
            result.AvailableYears.Last().Year.Should().Be(2025);

            result.RatingCategories.Should().HaveCount(2);
            result.RatingCategories.First().Name.Should().Be("GC");
            result.RatingCategories.Last().Name.Should().Be("Sprint");

            result.Ratings.Should().ContainSingle();
            result.Ratings.First().RatingCategoryId.Should().Be(2);
            result.Ratings.First().Rating.Should().Be(6);
            result.Ratings.First().CategoryName.Should().Be("Sprint");
            result.Ratings.First().Code.Should().Be("SPR");

            result.CompetitorInTeams.Should().ContainSingle();
            result.CompetitorInTeams.First().CompetitorInTeamId.Should().Be(100);
            result.CompetitorInTeams.First().TeamYearId.Should().Be(200);
            result.CompetitorInTeams.First().SeasonYearId.Should().Be(2);
            result.CompetitorInTeams.First().Year.Should().Be(2026);
            result.CompetitorInTeams.First().TeamName.Should().Be("UAE Team Emirates");
            result.CompetitorInTeams.First().IsNationalChampion.Should().BeTrue();
        }

        [Fact]
        public async Task Create_AddsCompetitorAndSaves()
        {
            var competitor = new Competitor();

            await _service.Create(competitor);

            _competitorRepositoryMock.Verify(
                x => x.Add(competitor),
                Times.Once);

            _competitorRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateCompetitorInTeam_AddsAndSaves()
        {
            var entity = new CompetitorInTeam();

            await _service.CreateCompetitorInTeam(entity);

            _competitorInTeamRepositoryMock.Verify(
                x => x.Add(entity),
                Times.Once);

            _competitorInTeamRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesCompetitorAndSaves()
        {
            var competitor = new Competitor();

            await _service.Delete(competitor);

            _competitorRepositoryMock.Verify(
                x => x.Remove(competitor),
                Times.Once);

            _competitorRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllCompetitors_ReturnsRepositoryResult()
        {
            const int seasonYearId = 2;

            var expected = new List<CompetitorDto>();

            _competitorRepositoryMock
                .Setup(x => x.GetAllCompetitors(seasonYearId))
                .ReturnsAsync(expected);

            var result = await _service.GetAllCompetitors(seasonYearId);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetAvailableYears_ReturnsRepositoryResult()
        {
            var expected = new List<SeasonYearDto>();

            _competitorRepositoryMock
                .Setup(x => x.GetAvailableSeasonYears())
                .ReturnsAsync(expected);

            var result = await _service.GetAvailableYears();

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetByTeamId_ReturnsRepositoryResult()
        {
            const int teamId = 10;

            var expected = new List<CompetitorInTeamDto>();

            _competitorRepositoryMock
                .Setup(x => x.GetByTeamId(teamId))
                .ReturnsAsync(expected);

            var result = await _service.GetByTeamId(teamId);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetCompetitorById_ReturnsRepositoryResult()
        {
            const int competitorId = 123;

            var expected = new Competitor
            {
                CompetitorId = competitorId
            };

            _competitorRepositoryMock
                .Setup(x => x.GetByCompetitorId(competitorId))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorById(competitorId);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetCompetitorsByCountry_ReturnsRepositoryResult()
        {
            const int countryId = 10;

            _competitorRepositoryMock
                .Setup(x => x.GetCompetitorsByCountry(countryId))
                .ReturnsAsync(42);

            var result = await _service.GetCompetitorsByCountry(countryId);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Update_UpdatesCompetitorAndSaves()
        {
            var competitor = new Competitor();

            await _service.Update(competitor);

            _competitorRepositoryMock.Verify(
                x => x.Update(competitor),
                Times.Once);

            _competitorRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetCompetitorByName_ReturnsRepositoryResult()
        {
            const string firstName = "Tadej";
            const string lastName = "Pogacar";
            const int countryId = 10;

            var expected = new Competitor
            {
                CompetitorId = 123
            };

            _competitorRepositoryMock
                .Setup(x => x.GetCompetitorByName(firstName, lastName, countryId))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorByName(
                firstName,
                lastName,
                countryId);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task CheckCompetitorInTeam_ReturnsRepositoryResult()
        {
            const int competitorId = 123;
            const int teamYearId = 456;

            _competitorInTeamRepositoryMock
                .Setup(x => x.CheckCompetitorInTeam(competitorId, teamYearId))
                .ReturnsAsync(true);

            var result = await _service.CheckCompetitorInTeam(
                competitorId,
                teamYearId);

            result.Should().BeTrue();
        }

        [Fact]
        public void GetCompetitorsByTerm_ReturnsRepositoryResult()
        {
            const string term = "Pog";

            var expected = new List<Competitor>
            {
                new Competitor
                {
                    CompetitorId = 123,
                    FirstName = "Tadej",
                    LastName = "Pogacar"
                }
            }.AsQueryable();

            _competitorRepositoryMock
                .Setup(x => x.GetCompetitorsByTerm(term))
                .Returns(expected);

            var result = _service.GetCompetitorsByTerm(term);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetCompetitorInTeamsByIdsAsync_ReturnsRepositoryResult()
        {
            var ids = new List<int> { 1, 2, 3 };

            var expected = new List<CompetitorInTeam>
            {
                new CompetitorInTeam
                {
                    Id = 1
                }
            };

            _competitorRepositoryMock
                .Setup(x => x.GetCompetitorInTeamsByIdsAsync(ids))
                .ReturnsAsync(expected);

            var result = await _service.GetCompetitorInTeamsByIdsAsync(ids);

            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetByTeamAndSeason_ReturnsRepositoryResult()
        {
            const int teamId = 10;
            const int seasonYearId = 2026;

            var expected = new List<CompetitorInTeamDto>();

            _competitorRepositoryMock
                .Setup(x => x.GetByTeamAndSeason(teamId, seasonYearId))
                .ReturnsAsync(expected);

            var result = await _service.GetByTeamAndSeason(
                teamId,
                seasonYearId);

            result.Should().BeSameAs(expected);
        }
    }
}