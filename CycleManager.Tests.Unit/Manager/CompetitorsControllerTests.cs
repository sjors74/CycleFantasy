using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;
using WebCycleManager.Models.ViewModel;

namespace CycleManager.Tests.Unit.Manager
{
    public class CompetitorsControllerTests
    {
        private readonly Mock<ICompetitorService> _competitorServiceMock;
        private readonly Mock<ITeamService> _teamServiceMock;
        private readonly Mock<ICountryService> _countryServiceMock;
        private readonly Mock<ISeasonYearService> _seasonYearServiceMock;
        private readonly Mock<IScraperService> _scraperServiceMock;
        private readonly CompetitorsController _controller;

        public CompetitorsControllerTests()
        {
            _competitorServiceMock = new Mock<ICompetitorService>();
            _teamServiceMock = new Mock<ITeamService>();
            _seasonYearServiceMock = new Mock<ISeasonYearService>();
            _countryServiceMock = new Mock<ICountryService>();
            _scraperServiceMock = new Mock<IScraperService>();

            _controller = new CompetitorsController(
                _competitorServiceMock.Object,
                _teamServiceMock.Object,
                _countryServiceMock.Object,
                _seasonYearServiceMock.Object,
                _scraperServiceMock.Object
            );
        }

        [Fact]
        public async Task Index_ReturnsView_WithCompetitorsList()
        {
            // Arrange
            var competitors = TestDataFactory.CreateCompetitorDtos(3);
            _competitorServiceMock
                .Setup(s => s.GetAvailableYears())
                .ReturnsAsync(new List<SeasonYearDto> 
                { 
                    new SeasonYearDto 
                    { 
                        SeasonYearId = 1,
                        Year = 2023,
                        Active = false
                    }, 
                    new SeasonYearDto 
                    {
                        SeasonYearId = 2,
                        Year = 2024,
                        Active = true
                    } 
                });
            _competitorServiceMock.Setup(s => s.GetAllCompetitors(It.IsAny<int>()))
                                  .ReturnsAsync(competitors);

            // Act
            var result = await _controller.Index(null, null, 1, 2);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorIndexViewModel>(view.Model);

            model.Competitors.Should().NotBeNull();
            model.Competitors.Should().HaveCount(3);
            model.SelectedSeasonYearId.Should().Be(2);
        }

        [Fact]
        public async Task Index_UsesCurrentFilter_WhenSearchStringIsNull()
        {
            // Arrange
            var competitors = TestDataFactory.CreateCompetitorDtos(2);

            _competitorServiceMock
                .Setup(s => s.GetAvailableYears())
                .ReturnsAsync(new List<SeasonYearDto>
                {
            new SeasonYearDto
            {
                SeasonYearId = 1,
                Year = 2023,
                Active = true
            }
                });

            _competitorServiceMock
                .Setup(s => s.GetAllCompetitors(1))
                .ReturnsAsync(competitors);

            // Act
            var result = await _controller.Index("Jansen", null, 1, null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorIndexViewModel>(view.Model);

            Assert.Equal("Jansen", model.CurrentFilter);
        }

        [Fact]
        public async Task Index_ReturnsConfigurationError_WhenNoActiveSeasonExists()
        {
            // Arrange
            _competitorServiceMock
                .Setup(s => s.GetAvailableYears())
                .ReturnsAsync(new List<SeasonYearDto>
                {
            new SeasonYearDto
            {
                SeasonYearId = 1,
                Year = 2025,
                Active = false
            }
                });

            // Act
            var result = await _controller.Index(null, null, null, null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("ConfigurationError", view.ViewName);
        }

        [Fact]
        public async Task Index_FiltersCompetitors_BySearchString()
        {
            // Arrange
            var competitors = new List<CompetitorDto>
            {
                new CompetitorDto
                {
                    CompetitorId = 1,
                    FirstName = "Jan",
                    LastName = "Jansen"
                },
                new CompetitorDto
                {
                    CompetitorId = 2,
                    FirstName = "Piet",
                    LastName = "Pietersen"
                }
            };

            _competitorServiceMock
                .Setup(s => s.GetAvailableYears())
                .ReturnsAsync(new List<SeasonYearDto>
                {
            new SeasonYearDto
            {
                SeasonYearId = 1,
                Year = 2025,
                Active = true
            }
                });

            _competitorServiceMock
                .Setup(s => s.GetAllCompetitors(1))
                .ReturnsAsync(competitors);

            // Act
            var result = await _controller.Index(null, "Jansen", null, 1);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompetitorIndexViewModel>(view.Model);

            model.Competitors.Should().HaveCount(1);
            model.Competitors.First().LastName.Should().Be("Jansen");
        }

        [Fact]
        public async Task Create_Get_ReturnsView_WithSeasonTeamsAndCountries()
        {
            // Arrange
            _seasonYearServiceMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<SeasonYearDto>
                {
            new SeasonYearDto
            {
                SeasonYearId = 1,
                Year = 2025,
                Active = true
            },
            new SeasonYearDto
            {
                SeasonYearId = 2,
                Year = 2024,
                Active = false
            }
                });

            _teamServiceMock
                .Setup(s => s.GetTeamYears(1))
                .ReturnsAsync(new List<TeamYearDto>
                {
            new TeamYearDto { TeamYearId = 2, Name = "Team B" },
            new TeamYearDto { TeamYearId = 1, Name = "Team A" }
                });

            _countryServiceMock
                .Setup(s => s.GetAll())
                .ReturnsAsync(new List<Country>
                {
            new Country { CountryId = 1, CountryNameLong = "Nederland" },
            new Country { CountryId = 2, CountryNameLong = "België" }
                });

            // Act
            var result = await _controller.Create((int?)null);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateCompetitorViewModel>(view.Model);

            Assert.Equal(1, model.SeasonYearId);
            Assert.Equal(2025, model.SeasonYear);

            Assert.Equal("-- Kies een team --", model.Teams.First().Text);
            Assert.Equal("Team A", model.Teams.Skip(1).First().Text);

            Assert.Equal("-- Kies een land --", model.Countries.First().Text);
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenSelectedCompetitorDoesNotExist()
        {
            // Arrange
            var model = TestDataFactory.CreateValidCreateCompetitorViewModel();
            model.CompetitorId = 99;

            _competitorServiceMock
                .Setup(s => s.GetCompetitorById(99))
                .ReturnsAsync((Competitor?)null);

            _seasonYearServiceMock
                .Setup(s => s.GetByIdAsync(model.SeasonYearId))
                .ReturnsAsync(new SeasonYear
                {
                    SeasonYearId = model.SeasonYearId,
                    Year = 2025,
                    Active = true
                });

            _teamServiceMock
                .Setup(s => s.GetTeamYears(model.SeasonYearId))
                .ReturnsAsync(new List<TeamYearDto>());

            _countryServiceMock
                .Setup(s => s.GetAll())
                .ReturnsAsync(new List<Country>());

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);

            Assert.Contains(
                _controller.ModelState[string.Empty]!.Errors,
                error => error.ErrorMessage == "Geselecteerde renner bestaat niet.");
        }

        [Fact]
        public async Task Create_Post_ExistingCompetitor_CreatesCompetitorInTeam()
        {
            // Arrange
            var model = TestDataFactory.CreateValidCreateCompetitorViewModel();
            model.CompetitorId = 0;

            var competitor = TestDataFactory.CreateCompetitor();

            _competitorServiceMock
                .Setup(s => s.GetCompetitorByName(
                    model.FirstName!,
                    model.LastName!,
                    model.CountryId))
                .ReturnsAsync(competitor);

            _competitorServiceMock
                .Setup(s => s.CheckCompetitorInTeam(
                    competitor.CompetitorId,
                    model.TeamYearId!.Value))
                .ReturnsAsync(false);

            _competitorServiceMock
                .Setup(s => s.CreateCompetitorInTeam(It.IsAny<CompetitorInTeam>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _competitorServiceMock.Verify(
                s => s.Create(It.IsAny<Competitor>()),
                Times.Never);

            _competitorServiceMock.Verify(
                s => s.CreateCompetitorInTeam(It.IsAny<CompetitorInTeam>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_ExistingCompetitor_CreatesCompetitorInTeam_AndRedirects()
        {
            // Arrange
            var model = TestDataFactory.CreateValidCreateCompetitorViewModel();
            model.CompetitorId = 10;
            model.TeamYearId = 5;

            var competitor = new Competitor
            {
                CompetitorId = 10,
                FirstName = "Jan",
                LastName = "Jansen"
            };

            _competitorServiceMock
                .Setup(s => s.GetCompetitorById(10))
                .ReturnsAsync(competitor);

            _competitorServiceMock
                .Setup(s => s.CheckCompetitorInTeam(
                    10,
                    5))
                .ReturnsAsync(false);

            _competitorServiceMock
                .Setup(s => s.CreateCompetitorInTeam(It.IsAny<CompetitorInTeam>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);

            _competitorServiceMock.Verify(
                s => s.GetCompetitorById(10),
                Times.Once);

            // Er mag géén nieuwe Competitor worden aangemaakt
            _competitorServiceMock.Verify(
                s => s.Create(It.IsAny<Competitor>()),
                Times.Never);

            // De koppeling Competitor -> TeamYear moet wel worden aangemaakt
            _competitorServiceMock.Verify(
                s => s.CreateCompetitorInTeam(
                    It.Is<CompetitorInTeam>(x =>
                        x.CompetitorId == 10 &&
                        x.TeamYearId == model.TeamYearId)),
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            var vm = TestDataFactory.CreateValidCreateCompetitorViewModel();
            var competitor = TestDataFactory.CreateCompetitor();

            _competitorServiceMock.Setup(s => s.GetCompetitorById(vm.CompetitorId))
                                  .ReturnsAsync((Competitor?)null);
            _competitorServiceMock.Setup(s => s.GetCompetitorByName(vm.FirstName!, vm.LastName!, vm.CountryId))
                                  .ReturnsAsync((Competitor?)null);
            _competitorServiceMock.Setup(s => s.Create(It.IsAny<Competitor>())).Returns(Task.CompletedTask);
            _competitorServiceMock.Setup(s => s.CheckCompetitorInTeam(It.IsAny<int>(), It.IsAny<int>()))
                                  .ReturnsAsync(false);
            _competitorServiceMock.Setup(s => s.CreateCompetitorInTeam(It.IsAny<CompetitorInTeam>()))
                                  .Returns(Task.CompletedTask);

            var result = await _controller.Create(vm);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            _competitorServiceMock
               .Setup(s => s.GetAllCompetitors(It.IsAny<int>()))
                .ReturnsAsync(new List<CompetitorDto>
                   {
                        new CompetitorDto { CompetitorId = 1, FirstName = "Jan", LastName = "Jansen" },
                        new CompetitorDto { CompetitorId = 2, FirstName = "Piet", LastName = "Pietersen" }
                    });

            _teamServiceMock
                .Setup(s => s.GetTeamYears(It.IsAny<int>()))
                .ReturnsAsync(new List<TeamYearDto>
                {
                    new TeamYearDto { TeamYearId = 1, Name = "Team A" },
                    new TeamYearDto { TeamYearId = 2, Name = "Team B" }
                });

            _countryServiceMock
                .Setup(s => s.GetAll())
                .ReturnsAsync(new List<Country>
                {
                    new Country { CountryId = 1, CountryNameLong = "Nederland" },
                    new Country { CountryId = 2, CountryNameLong = "België" }
                });

            _controller.ModelState.AddModelError("FirstName", "Required");
            _controller.ModelState.AddModelError("LastName", "Required");

            var model = new CreateCompetitorViewModel
            {
                CompetitorId = 0,
                FirstName = "", // invalid
                LastName = "",  // invalid
                TeamYearId = 1,
                CountryId = 1,
            };

            // Act
            var actionResult = await _controller.Create(model);

            Assert.NotNull(actionResult);

            var result = Assert.IsType<ViewResult>(actionResult);


            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);

            var resultModel = Assert.IsType<CreateCompetitorViewModel>(result.Model);

            Assert.NotNull(resultModel.Teams);
            Assert.NotEmpty(resultModel.Teams);
            Assert.Contains(resultModel.Teams, t => t.Text == "Team A");
            Assert.Contains(resultModel.Teams, t => t.Text == "Team B");

            Assert.NotNull(resultModel.Countries);
            Assert.NotEmpty(resultModel.Countries);
            Assert.Contains(resultModel.Countries, c => c.Text == "Nederland");
            Assert.Contains(resultModel.Countries, c => c.Text == "België");

            Assert.NotNull(_controller.ViewBag.Competitors);
        }

        [Fact]
        public async Task Details_ReturnsView_WhenCompetitorFound()
        {
            var competitor = TestDataFactory.CreateCompetitor();
            _competitorServiceMock.Setup(s => s.GetCompetitorById(1)).ReturnsAsync(competitor);

            var result = await _controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            view.Model.Should().BeAssignableTo<Competitor>();
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNullOrMissing()
        {
            var result = await _controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenCompetitorDoesNotExist()
        {
            _competitorServiceMock
                .Setup(s => s.GetCompetitorForEdit(1))
                .ReturnsAsync((CompetitorEditDto?)null);

            var result = await _controller.Edit(1, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsNotFound_WhenCompetitorDoesNotExist()
        {
            // Arrange
            _controller.ModelState.AddModelError("FirstName", "Required");

            var input = new CompetitorEditInputModel
            {
                CompetitorId = 1,
                FirstName = "",
                LastName = "Test"
            };

            _competitorServiceMock
                .Setup(s => s.GetCompetitorForEdit(1))
                .ReturnsAsync((CompetitorEditDto?)null);

            // Act
            var result = await _controller.Edit(input);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var input = TestDataFactory.CreateCompetitorEditInputModel();

            _competitorServiceMock
                .Setup(s => s.UpdateCompetitorWithTeam(It.IsAny<CompetitorEditDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(input);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _competitorServiceMock.Verify(s => s.UpdateCompetitorWithTeam(It.IsAny<CompetitorEditDto>()), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            _controller.ModelState.AddModelError("FirstName", "Required");

            var input = new CompetitorEditInputModel
            {
                CompetitorId = 1,
                FirstName = "",
                LastName = "Test"
            };

            var dto = TestDataFactory.CreateCompetitorEditDto();
            _competitorServiceMock.Setup(s => s.GetCompetitorForEdit(1)).ReturnsAsync(dto);

            // Act
            var result = await _controller.Edit(input);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<CompetitorEditViewModel>(view.Model);
            Assert.Equal(dto.CompetitorId, model.CompetitorId);
        }

        [Fact]
        public async Task Delete_Get_ReturnsView_WhenCompetitorFound()
        {
            var competitor = TestDataFactory.CreateCompetitor();
            _competitorServiceMock.Setup(s => s.GetCompetitorById(1))
                                  .ReturnsAsync(competitor);

            var result = await _controller.Delete(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Competitor>(view.Model);
            Assert.Equal(competitor.CompetitorId, model.CompetitorId);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNullOrNotFound()
        {
            var result1 = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result1);

            _competitorServiceMock.Setup(s => s.GetCompetitorById(1))
                                  .ReturnsAsync((Competitor?)null);

            var result2 = await _controller.Delete(1);
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public async Task DeleteConfirmed_DoesNothing_WhenCompetitorDoesNotExist()
        {
            // Arrange
            _competitorServiceMock
                .Setup(s => s.GetCompetitorById(999))
                .ReturnsAsync((Competitor?)null);

            // Act
            var result = await _controller.DeleteConfirmed(999);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _competitorServiceMock.Verify(
                s => s.Delete(It.IsAny<Competitor>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesCompetitor_AndRedirects()
        {
            var competitor = TestDataFactory.CreateCompetitor();
            _competitorServiceMock.Setup(s => s.GetCompetitorById(1)).ReturnsAsync(competitor);
            _competitorServiceMock.Setup(s => s.Delete(competitor)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task SearchCompetitors_ReturnsJsonResult()
        {
            // Arrange
            var mockCompetitors = new List<Competitor>
            {
                new Competitor { CompetitorId = 1, FirstName = "Jan", LastName = "Jansen" },
                new Competitor { CompetitorId = 2, FirstName = "Piet", LastName = "Pietersen" }
            };

            var asyncCompetitors = new TestAsyncEnumerable<Competitor>(mockCompetitors);

            // Setup mock: maak het IAsyncEnumerable compatibel met ToListAsync()
            _competitorServiceMock
                .Setup(s => s.GetCompetitorsByTerm(It.IsAny<string>()))
                .Returns((string term) =>
                {
                    var filtered = mockCompetitors
                        .Where(c => $"{c.FirstName} {c.LastName}"
                            .Contains(term, StringComparison.OrdinalIgnoreCase))
                        .AsQueryable();
                    return new TestAsyncEnumerable<Competitor>(filtered);
                });

            // Act
            var result = await _controller.SearchCompetitors("Ja");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);

            Assert.Contains(data, d => d.ToString()?.Contains("Jan Jansen") == true);
            Assert.DoesNotContain(data, d => d.ToString()?.Contains("Piet Pietersen") == true);
        }

        [Fact]
        public async Task GetCompetitorInfo_ReturnsJson_WhenFound()
        {
            var competitor = TestDataFactory.CreateCompetitorWithTeam();
            _competitorServiceMock.Setup(s => s.GetCompetitorById(1))
                                  .ReturnsAsync(competitor);

            var result = await _controller.GetCompetitorInfo(1, competitor.CompetitorInTeams.First().TeamYearId);

            var json = Assert.IsType<JsonResult>(result);
            json.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCompetitorInfo_ReturnsNotFound_WhenCompetitorDoesNotExist()
        {
            _competitorServiceMock
                .Setup(s => s.GetCompetitorById(999))
                .ReturnsAsync((Competitor?)null);

            var result = await _controller.GetCompetitorInfo(999, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RunRatingCompetitorScrape_Success_RedirectsToEdit()
        {
            // Arrange
            _scraperServiceMock
                .Setup(s => s.RunRatingCompetitorScrapeAsync(1))
                .Returns(Task.CompletedTask);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            // Act
            var result = await _controller.RunRatingCompetitorScrape(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal(1, redirect.RouteValues!["id"]);

            Assert.Equal(
                "Cycling Flash ratings succesvol opgehaald.",
                _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task RunRatingCompetitorScrape_Exception_SetsErrorMessage()
        {
            // Arrange
            _scraperServiceMock
                .Setup(s => s.RunRatingCompetitorScrapeAsync(1))
                .ThrowsAsync(new Exception("Test error"));

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            // Act
            var result = await _controller.RunRatingCompetitorScrape(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal(1, redirect.RouteValues!["id"]);

            Assert.Equal(
                "Het ophalen van de Cycling Flash ratings is mislukt.",
                _controller.TempData["ErrorMessage"]);
        }
    }
}
