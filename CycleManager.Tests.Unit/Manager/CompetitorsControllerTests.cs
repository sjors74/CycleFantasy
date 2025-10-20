using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Domain.Dto;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly CompetitorsController _controller;

        public CompetitorsControllerTests()
        {
            _competitorServiceMock = new Mock<ICompetitorService>();
            _teamServiceMock = new Mock<ITeamService>();
            _countryServiceMock = new Mock<ICountryService>();

            _controller = new CompetitorsController(
                _competitorServiceMock.Object,
                _teamServiceMock.Object,
                _countryServiceMock.Object
            );
        }

        [Fact]
        public async Task Index_ReturnsView_WithCompetitorsList()
        {
            // Arrange
            var competitors = TestDataFactory.CreateCompetitorDtos(3);
            _competitorServiceMock.Setup(s => s.GetAvailableYears()).ReturnsAsync(new List<int> { 2023, 2024 });
            _competitorServiceMock.Setup(s => s.GetAllCompetitors(It.IsAny<int>()))
                                  .ReturnsAsync(competitors);

            // Act
            var result = await _controller.Index(null, null, 1, 2024);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            view.Model.Should().NotBeNull();
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
        public async Task Create_Get_ReturnsView_WithViewBags()
        {
            // arrange
            _competitorServiceMock.Setup(s => s.GetAllCompetitors(It.IsAny<int>()))
                                  .ReturnsAsync(new List<CompetitorDto>());
            _teamServiceMock.Setup(s => s.GetAllTeams())
                            .ReturnsAsync(TestDataFactory.FakeTeams());
            _countryServiceMock.Setup(s => s.GetAll())
                               .ReturnsAsync(TestDataFactory.FakeCountries());

            // act
            var result = await _controller.Create();

            // assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view.ViewData);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            var vm = TestDataFactory.CreateValidCreateCompetitorViewModel();
            var competitor = TestDataFactory.CreateCompetitor();
            _competitorServiceMock.Setup(s => s.GetCompetitorById(vm.CompetitorId))
                                  .ReturnsAsync((Competitor)null);
            _competitorServiceMock.Setup(s => s.GetCompetitorByName(vm.FirstName, vm.LastName, vm.CountryId))
                                  .ReturnsAsync((Competitor)null);
            _competitorServiceMock.Setup(s => s.Create(It.IsAny<Competitor>())).Returns(Task.CompletedTask);
            _competitorServiceMock.Setup(s => s.CheckCompetitorInTeam(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
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
                .Setup(s => s.GetAllTeams())
                .ReturnsAsync(new List<Team>
                {
                    new Team { TeamId = 1, CurrentTeamName = "Team A" },
                    new Team { TeamId = 2, CurrentTeamName = "Team B" }
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
                TeamId = 1,
                CountryId = 1,
                Year = 2025
            };

            // Act
            var result = await _controller.Create(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, result.Model);

            Assert.NotNull(_controller.ViewData["TeamId"]);
            Assert.NotNull(_controller.ViewData["CountryId"]);
            Assert.NotNull(_controller.ViewBag.Competitors);
        }

        [Fact]
        public async Task Edit_Get_ReturnsView_WhenCompetitorFound()
        {
            var dto = TestDataFactory.CreateCompetitorEditDto();
            _competitorServiceMock.Setup(s => s.GetCompetitorForEdit(1)).ReturnsAsync(dto);

            var result = await _controller.Edit(1);

            var view = Assert.IsType<ViewResult>(result);
            view.Model.Should().BeAssignableTo<CompetitorEditViewModel>();
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenMissing()
        {
            _competitorServiceMock.Setup(s => s.GetCompetitorForEdit(1))
                                  .ReturnsAsync((CompetitorEditDto)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
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

            // Setup mock: maak het IAsyncEnumerable compatibel met ToListAsync()
            _competitorServiceMock
                .Setup(s => s.GetCompetitorsByTerm(It.IsAny<string>()))
                .Returns(mockCompetitors.AsQueryable());

            // Act
            var result = await _controller.SearchCompetitors("Ja");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);

            Assert.Contains(data, d => d.ToString().Contains("Jan Jansen"));
            Assert.Contains(data, d => d.ToString().Contains("Piet Pietersen"));
        }

        [Fact]
        public async Task GetCompetitorInfo_ReturnsJson_WhenFound()
        {
            var competitor = TestDataFactory.CreateCompetitorWithTeam();
            _competitorServiceMock.Setup(s => s.GetCompetitorById(1))
                                  .ReturnsAsync(competitor);

            var result = await _controller.GetCompetitorInfo(1, competitor.CompetitorInTeams.First().Year);

            var json = Assert.IsType<JsonResult>(result);
            json.Value.Should().NotBeNull();
        }
    }
}
