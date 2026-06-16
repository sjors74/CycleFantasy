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
        private readonly TeamsController _controller;

        public TeamsControllerTests()
        {
            _mockTeamService = new Mock<ITeamService>();
            _mockCountryService = new Mock<ICountryService>();
            _controller = new TeamsController(_mockTeamService.Object, _mockCountryService.Object);
        }

        // -------------------------------------------
        // INDEX
        // -------------------------------------------
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

        // -------------------------------------------
        // DETAILS
        // -------------------------------------------
        [Fact]
        public async Task Details_ReturnsNotFound_WhenTeamNotExists()
        {
            _mockTeamService.Setup(s => s.GetTeamForCurrentYear(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((Team)null);

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

        // -------------------------------------------
        // CREATE (POST)
        // -------------------------------------------
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

        // -------------------------------------------
        // EDIT (GET)
        // -------------------------------------------
        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenTeamNotFound()
        {
            _mockTeamService.Setup(s => s.GetTeamById(It.IsAny<int>())).ReturnsAsync((Team)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithCorrectModel()
        {
            var team = TestDataFactory.FakeTeamWithYears();
            _mockTeamService.Setup(s => s.GetTeamById(1)).ReturnsAsync(team);
            _mockCountryService.Setup(s => s.GetAll()).ReturnsAsync(TestDataFactory.FakeCountries());

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TeamEditViewModel>(viewResult.Model);
            Assert.Equal("Team Edit", model.CurrentTeamName);
            Assert.Equal(1, model.TeamYears.Count);
        }

        // -------------------------------------------
        // EDIT (POST)
        // -------------------------------------------
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
                AvailableYears = new List<int> { 2025 }
            };

            _mockTeamService.Setup(s => s.GetTeamById(1)).ReturnsAsync(team);
            _mockTeamService.Setup(s => s.Update(It.IsAny<Team>())).Returns(Task.CompletedTask);
            _mockCountryService.Setup(s => s.GetAll()).ReturnsAsync(TestDataFactory.FakeCountries());

            var result = await _controller.Edit(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _mockTeamService.Verify(s => s.Update(It.IsAny<Team>()), Times.Once);
        }

        // -------------------------------------------
        // DELETE
        // -------------------------------------------
        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdNullOrTeamMissing()
        {
            _mockTeamService.Setup(s => s.GetTeamById(It.IsAny<int>())).ReturnsAsync((Team)null);

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
    }
}
