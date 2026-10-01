using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class SeasonYearsControllerTests
    {
        private readonly Mock<ISeasonYearService> _mockSeasonYearService;
        private readonly SeasonYearsController _controller;

        public SeasonYearsControllerTests()
        {
            _mockSeasonYearService = new Mock<ISeasonYearService>();
            _controller = new SeasonYearsController(_mockSeasonYearService.Object);
        }

        // -------------------------------------------
        // INDEX
        // -------------------------------------------
        [Fact]
        public async Task Index_ReturnsView_WithSeasonYearViewModels()
        {
            var years = new List<SeasonYearDto>
            {
                new() { SeasonYearId = 1, Year = 2024, Active = true },
                new() { SeasonYearId = 2, Year = 2026, Active = true },
                new() { SeasonYearId = 3, Year = 2025, Active = false }
            };

            _mockSeasonYearService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(years);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<SeasonYearViewModel>>(viewResult.Model);

            Assert.Equal(3, model.Count());
            Assert.Equal(2026, model.First().Year);
            Assert.Equal(2024, model.Last().Year);
        }

        // -------------------------------------------
        // CREATE (GET)
        // -------------------------------------------
        [Fact]
        public void Create_Get_ReturnsView_WithActiveTrue()
        {
            var result = _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SeasonYearViewModel>(viewResult.Model);

            Assert.True(model.Active);
        }

        // -------------------------------------------
        // CREATE (POST)
        // -------------------------------------------
        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Year", "Required");

            var model = new SeasonYearViewModel
            {
                Year = 0,
                Active = true
            };

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<SeasonYearViewModel>(viewResult.Model);

            _mockSeasonYearService.Verify(
                s => s.CreateAsync(It.IsAny<SeasonYear>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_ValidModel_CreatesAndRedirects()
        {
            var model = new SeasonYearViewModel
            {
                Year = 2026,
                Active = true
            };

            _mockSeasonYearService
                .Setup(s => s.CreateAsync(It.IsAny<SeasonYear>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Create(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _mockSeasonYearService.Verify(
                s => s.CreateAsync(It.Is<SeasonYear>(x =>
                    x.Year == 2026 &&
                    x.Active)),
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_ServiceThrows_ReturnsViewWithModelError()
        {
            var model = new SeasonYearViewModel
            {
                Year = 2026,
                Active = true
            };

            _mockSeasonYearService
                .Setup(s => s.CreateAsync(It.IsAny<SeasonYear>()))
                .ThrowsAsync(new Exception("Test error"));

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);

            Assert.Contains(
                _controller.ModelState[string.Empty]!.Errors,
                error => error.ErrorMessage == "Test error");
        }

        // -------------------------------------------
        // EDIT (GET)
        // -------------------------------------------
        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenSeasonYearNotFound()
        {
            _mockSeasonYearService
                .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((SeasonYear?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithCorrectModel()
        {
            var year = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            _mockSeasonYearService
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(year);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SeasonYearViewModel>(viewResult.Model);

            Assert.Equal(1, model.SeasonYearId);
            Assert.Equal(2026, model.Year);
            Assert.True(model.Active);
        }

        // -------------------------------------------
        // EDIT (POST)
        // -------------------------------------------
        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Year", "Required");

            var model = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 0,
                Active = true
            };

            var result = await _controller.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);

            _mockSeasonYearService.Verify(
                s => s.UpdateAsync(It.IsAny<SeasonYear>()),
                Times.Never);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
        {
            var model = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            _mockSeasonYearService
                .Setup(s => s.UpdateAsync(It.IsAny<SeasonYear>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Edit(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _mockSeasonYearService.Verify(
                s => s.UpdateAsync(It.Is<SeasonYear>(x =>
                    x.SeasonYearId == 1 &&
                    x.Year == 2026 &&
                    x.Active)),
                Times.Once);
        }

        // -------------------------------------------
        // DELETE (GET)
        // -------------------------------------------
        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenSeasonYearNotFound()
        {
            _mockSeasonYearService
                .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((SeasonYear?)null);

            var result = await _controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewResult_WithCorrectModel()
        {
            var year = new SeasonYear
            {
                SeasonYearId = 1,
                Year = 2026,
                Active = true
            };

            _mockSeasonYearService
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(year);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SeasonYearViewModel>(viewResult.Model);

            Assert.Equal(1, model.SeasonYearId);
            Assert.Equal(2026, model.Year);
            Assert.True(model.Active);
        }

        // -------------------------------------------
        // DELETE CONFIRMED
        // -------------------------------------------
        [Fact]
        public async Task DeleteConfirmed_DeletesSeasonYear_AndRedirects()
        {
            _mockSeasonYearService
                .Setup(s => s.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _mockSeasonYearService.Verify(
                s => s.DeleteAsync(1),
                Times.Once);
        }

        // -------------------------------------------
        // GET SEASON YEARS
        // -------------------------------------------
        [Fact]
        public async Task GetSeasonYears_ReturnsOk_WithSeasonYears()
        {
            var years = new List<SeasonYearDto>
            {
                new() { SeasonYearId = 1, Year = 2025, Active = true },
                new() { SeasonYearId = 2, Year = 2026, Active = true }
            };

            _mockSeasonYearService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(years);

            var result = await _controller.GetSeasonYears();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<SeasonYearDto>>(okResult.Value);

            Assert.Equal(2, model.Count());
        }
    }
}