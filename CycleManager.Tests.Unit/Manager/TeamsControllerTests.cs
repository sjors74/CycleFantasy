using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class TeamsControllerTests
    {
        private readonly Mock<ITeamService> _mockTeamService;
        private readonly Mock<ICountryService> _mockCountryService;
        private readonly Mock<ISeasonYearService> _mockSeasonYearService;
        private readonly TeamsController _controller;

        public TeamsControllerTests()
        {
            _mockTeamService = new Mock<ITeamService>();
            _mockCountryService = new Mock<ICountryService>();
            _mockSeasonYearService = new Mock<ISeasonYearService>();
            _controller = new TeamsController(_mockTeamService.Object, _mockCountryService.Object, _mockSeasonYearService.Object);
        }

        #region Index Tests
        [Fact]
        public async Task Index_ReturnsView_WithTeamViewModels()
        {
            var teams = TestDataFactory.FakeTeams();
            _mockTeamService.Setup(s => s.GetAllTeams()).ReturnsAsync(teams);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TeamViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Index_ReturnsCorrectViewModels_AndScrapingStatus()
        {
            var teams = TestDataFactory.FakeTeams();

            _mockTeamService
                .Setup(s => s.GetAllTeams())
                .ReturnsAsync(teams);

            _mockTeamService
                .Setup(s => s.HasUnprocessedScrapedTeams())
                .ReturnsAsync(true);

            _mockTeamService
                .Setup(s => s.CountUnprocessedScrapedCompetitors())
                .ReturnsAsync(7);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TeamViewModel>>(viewResult.Model);

            Assert.Equal(2, model.Count());

            Assert.True(_controller.ViewBag.HasUnprocessedScraped);
            Assert.Equal(7, _controller.ViewBag.UnprocessedScrapedCount);

            var firstTeam = model.First();

            Assert.NotNull(firstTeam);
            Assert.False(string.IsNullOrWhiteSpace(firstTeam.TeamName));
        }

        [Fact]
        public async Task Index_HandlesTeamWithoutCountryOrActiveTeamYear()
        {
            var teams = new List<Team>
            {
                new Team
                {
                    TeamId = 1,
                    CurrentTeamName = "No Country Team",
                    PcsName = "PCS",
                    Country = null,
                    TeamYears = new List<TeamYear>()
                }
            };

            _mockTeamService
                .Setup(s => s.GetAllTeams())
                .ReturnsAsync(teams);

            _mockTeamService
                .Setup(s => s.HasUnprocessedScrapedTeams())
                .ReturnsAsync(false);

            _mockTeamService
                .Setup(s => s.CountUnprocessedScrapedCompetitors())
                .ReturnsAsync(0);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TeamViewModel>>(viewResult.Model);

            var team = Assert.Single(model);

            Assert.Equal("No Country Team", team.TeamName);
            Assert.Equal(string.Empty, team.CountryNameShort);
            Assert.Equal(0, team.CompetitorsInTeam);
        }
        #endregion
        #region Details Tests

        [Fact]
        public async Task Details_ReturnsNotFound_WhenTeamNotExists()
        {
            _mockTeamService.Setup(s => s.GetTeamForCurrentYear(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var result = await _controller.Details(1, 2024);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WhenTeamExists()
        {
            var team = TestDataFactory.FakeTeamWithCompetitors(2024);
            _mockTeamService.Setup(s => s.GetTeamForCurrentYear(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(team);

            var result = await _controller.Details(1, 2024);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamDetailsViewModel>(viewResult.Model);
            Assert.Equal("TestTeam", model.TeamName);
        }

        [Fact]
        public async Task Details_UsesCurrentYear_WhenYearIsNotProvided()
        {
            var currentYear = DateTime.Now.Year;
            var team = TestDataFactory.FakeTeamWithCompetitors(currentYear);

            _mockTeamService
                .Setup(s => s.GetTeamForCurrentYear(1, currentYear))
                .ReturnsAsync(team);

            var result = await _controller.Details(1, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamDetailsViewModel>(viewResult.Model);

            Assert.Equal(currentYear, model.SelectedYear);

            _mockTeamService.Verify(
                s => s.GetTeamForCurrentYear(1, currentYear),
                Times.Once);
        }

        [Fact]
        public async Task Details_HandlesCompetitorWithoutCountry_AndSortsByLastName()
        {
            var currentYear = 2024;

            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TestTeam",
                TeamYears = new List<TeamYear>
                {
                    new TeamYear
                    {
                        TeamYearId = 1,
                        SeasonYearId = 1,
                        SeasonYear = new SeasonYear
                        {
                            Year = currentYear,
                            Active = true
                        },
                        CompetitorInTeams = new List<CompetitorInTeam>
                        {
                            new CompetitorInTeam
                            {
                                CompetitorId = 1,
                                IsNationalChampion = false,
                                Competitor = new Competitor
                                {
                                    CompetitorId = 1,
                                    FirstName = "Jan",
                                    LastName = "Zand",
                                    Country = null!
                                }
                            },
                            new CompetitorInTeam
                            {
                                CompetitorId = 2,
                                IsNationalChampion = true,
                                Competitor = new Competitor
                                {
                                    CompetitorId = 2,
                                    FirstName = "Piet",
                                    LastName = "Bakker",
                                    Country = null!
                                }
                            }
                        }
                    }
                }
            };

            _mockTeamService
                .Setup(s => s.GetTeamForCurrentYear(1, currentYear))
                .ReturnsAsync(team);

            var result = await _controller.Details(1, currentYear);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamDetailsViewModel>(viewResult.Model);

            Assert.Equal(2, model.Competitors.Count);

            // Sorted by LastName
            Assert.Equal("Bakker", model.Competitors[0].LastName);
            Assert.Equal("Zand", model.Competitors[1].LastName);

            // Country fallback
            Assert.Equal("onbekend", model.Competitors[0].Land);
            Assert.Equal("onbekend", model.Competitors[1].Land);

            Assert.True(model.Competitors[0].IsNationalChampion);
            Assert.False(model.Competitors[1].IsNationalChampion);
        }

        [Fact]
        public async Task Details_OnlyIncludesCompetitorsFromSelectedYear_AndReturnsAvailableYears()
        {
            var team = new Team
            {
                TeamId = 1,
                CurrentTeamName = "TestTeam",
                TeamYears = new List<TeamYear>
                {
                    new TeamYear
                    {
                        TeamYearId = 1,
                        SeasonYearId = 1,
                        SeasonYear = new SeasonYear
                        {
                            Year = 2024,
                            Active = true
                        },
                        CompetitorInTeams = new List<CompetitorInTeam>
                        {
                            new CompetitorInTeam
                            {
                                CompetitorId = 1,
                                Competitor = new Competitor
                                {
                                    CompetitorId = 1,
                                    FirstName = "Jan",
                                    LastName = "Bakker"
                                }
                            }
                        }
                    },
                    new TeamYear
                    {
                        TeamYearId = 2,
                        SeasonYearId = 2,
                        SeasonYear = new SeasonYear
                        {
                            Year = 2025,
                            Active = false
                        },
                        CompetitorInTeams = new List<CompetitorInTeam>
                        {
                            new CompetitorInTeam
                            {
                                CompetitorId = 2,
                                Competitor = new Competitor
                                {
                                    CompetitorId = 2,
                                    FirstName = "Piet",
                                    LastName = "Jansen"
                                }
                            }
                        }
                    },
                    new TeamYear
                    {
                        TeamYearId = 3,
                        SeasonYearId = 3,
                        SeasonYear = new SeasonYear
                        {
                            Year = 2023,
                            Active = false
                        },
                        CompetitorInTeams = new List<CompetitorInTeam>()
                    }
                }
            };

            _mockTeamService
                .Setup(s => s.GetTeamForCurrentYear(1, 2024))
                .ReturnsAsync(team);

            var result = await _controller.Details(1, 2024);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamDetailsViewModel>(viewResult.Model);

            // Alleen competitors van het geselecteerde jaar
            var competitor = Assert.Single(model.Competitors);
            Assert.Equal("Bakker", competitor.LastName);

            // Alle beschikbare jaren
            Assert.Equal(
                new[] { 2025, 2024, 2023 },
                model.AvailableYears);
        }
        #endregion

        #region Create Tests
        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithCountries()
        {
            _controller.ModelState.AddModelError("CurrentTeamName", "Required");
            _mockCountryService.Setup(s => s.GetAll()).ReturnsAsync(TestDataFactory.FakeCountries());

            var vm = new TeamCreateViewModel();

            var result = await _controller.Create(vm);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamCreateViewModel>(viewResult.Model);
            Assert.NotEmpty(model.Countries);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            var vm = new TeamCreateViewModel
            {
                CurrentTeamName = "New Team",
                PcsName = "PCS",
                CountryId = 1
            };

            _mockTeamService.Setup(s => s.Add(It.IsAny<Team>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(vm);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockTeamService.Verify(s => s.Add(It.IsAny<Team>()), Times.Once);
        }

        #endregion
        #region Edit Tests

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenTeamNotFound()
        {
            _mockTeamService
                .Setup(s => s.GetTeamById(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithCorrectModel()
        {
            // Arrange
            var team = TestDataFactory.FakeTeamWithYears();
            _mockTeamService.Setup(s => s.GetTeamById(1)).ReturnsAsync(team);
            _mockCountryService.Setup(s => s.GetAll()).ReturnsAsync(TestDataFactory.FakeCountries());

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamEditViewModel>(viewResult.Model);
            Assert.Equal("Team Edit", model.CurrentTeamName);
            Assert.Single(model.TeamYears);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("CurrentTeamName", "Required");
            var model = new TeamEditViewModel();

            _mockCountryService.Setup(s => s.GetAll()).ReturnsAsync(TestDataFactory.FakeCountries());

            var result = await _controller.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<TeamEditViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
        {
            var team = TestDataFactory.FakeTeamWithYears();
            var model = new TeamEditViewModel
            {
                TeamId = 1,
                CurrentTeamName = "NewName",
                PcsName = "NewPCS",
                CountryId = 2,
                TeamYears = new List<TeamYearViewModel>(),
            };

            _mockTeamService.Setup(s => s.GetTeamById(1)).ReturnsAsync(team);
            _mockTeamService.Setup(s => s.Update(It.IsAny<Team>())).Returns(Task.CompletedTask);
            _mockCountryService.Setup(s => s.GetAll()).ReturnsAsync(TestDataFactory.FakeCountries());

            var result = await _controller.Edit(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _mockTeamService.Verify(s => s.Update(It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenTeamDoesNotExist()
        {
            var model = new TeamEditViewModel
            {
                TeamId = 999,
                CurrentTeamName = "Unknown Team",
                PcsName = "PCS",
                CountryId = 1,
                TeamYears = new List<TeamYearViewModel>()
            };

            _mockTeamService
                .Setup(s => s.GetTeamById(999))
                .ReturnsAsync((Team?)null);

            var result = await _controller.Edit(model);

            Assert.IsType<NotFoundResult>(result);

            _mockTeamService.Verify(
                s => s.Update(It.IsAny<Team>()),
                Times.Never);
        }

        [Fact]
        public async Task Edit_Post_Throws_WhenPostedTeamYearDoesNotExist()
        {
            var team = TestDataFactory.FakeTeamWithYears();

            _mockTeamService
                .Setup(s => s.GetTeamById(1))
                .ReturnsAsync(team);

            var model = new TeamEditViewModel
            {
                TeamId = 1,
                CurrentTeamName = "TestTeam",
                PcsName = "PCS",
                CountryId = 1,
                TeamYears = new List<TeamYearViewModel>
                {
                    new TeamYearViewModel
                    {
                        SeasonYearId = 999,
                        Name = "Bestaat niet"
                    }
                }
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Edit(model));

            Assert.Equal(
                "Geen TeamYear gevonden voor seizoen 999.",
                exception.Message);

            _mockTeamService.Verify(
                s => s.Update(It.IsAny<Team>()),
                Times.Never);
        }

        [Fact]
        public async Task Edit_Post_RemovesTeamYearsThatAreNotPosted()
        {
            var team = TestDataFactory.FakeTeamWithYears();

            var teamYearToKeep = team.TeamYears.First();
            var teamYearToRemove = team.TeamYears.Last();

            _mockTeamService
                .Setup(s => s.GetTeamById(1))
                .ReturnsAsync(team);

            _mockTeamService
                .Setup(s => s.Update(It.IsAny<Team>()))
                .Returns(Task.CompletedTask);

            var model = new TeamEditViewModel
            {
                TeamId = 1,
                CurrentTeamName = "Updated Team",
                PcsName = "PCS",
                CountryId = 1,
                TeamYears = new List<TeamYearViewModel>
                {
                    new TeamYearViewModel
                    {
                        SeasonYearId = teamYearToKeep.SeasonYearId,
                        Name = "Updated Name"
                    }
                }
            };

            var result = await _controller.Edit(model);

            Assert.IsType<RedirectToActionResult>(result);

            Assert.Single(team.TeamYears);
            Assert.Same(teamYearToKeep, team.TeamYears.Single());

            Assert.Equal("Updated Name", teamYearToKeep.Name);

            _mockTeamService.Verify(
                s => s.Update(team),
                Times.Once);
        }
        #endregion

        #region Delete Tests
        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdNullOrTeamMissing()
        {
            _mockTeamService
                .Setup(s => s.GetTeamById(It.IsAny<int>()))
                .ReturnsAsync((Team?)null);

            var resultNullId = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(resultNullId);

            var resultNoTeam = await _controller.Delete(1);
            Assert.IsType<NotFoundResult>(resultNoTeam);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewResult_WhenTeamExists()
        {
            var team = new Team { TeamId = 1, CurrentTeamName = "DeleteTeam" };
            _mockTeamService.Setup(s => s.GetTeamById(1)).ReturnsAsync(team);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamDeleteViewModel>(viewResult.Model);
            Assert.Equal("DeleteTeam", model.CurrentTeamName);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesTeam_AndRedirects()
        {
            var team = new Team { TeamId = 1, CurrentTeamName = "DeleteTeam" };
            _mockTeamService.Setup(s => s.GetTeamById(1)).ReturnsAsync(team);
            _mockTeamService.Setup(s => s.Delete(team)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _mockTeamService.Verify(s => s.Delete(It.IsAny<Team>()), Times.Once);
        }

        #endregion
    }
}
