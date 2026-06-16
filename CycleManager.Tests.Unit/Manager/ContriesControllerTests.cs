using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebCycleManager.Controllers;
using WebCycleManager.Models;

namespace CycleManager.Tests.Unit.Manager
{
    public class CountriesControllerTests
    {
        private readonly Mock<ICountryService> _countryServiceMock;
        private readonly Mock<ICompetitorService> _competitorServiceMock;
        private readonly CountriesController _controller;

        public CountriesControllerTests()
        {
            _countryServiceMock = new Mock<ICountryService>();
            _competitorServiceMock = new Mock<ICompetitorService>();
            _controller = new CountriesController(_competitorServiceMock.Object, _countryServiceMock.Object);
        }

        // =====================
        // INDEX
        // =====================
        [Fact]
        public async Task Index_ReturnsViewWithCountryViewModels()
        {
            // Arrange
            var countries = new List<Country>
            {
                new Country { CountryId = 1, CountryNameLong = "Belgium", CountryNameShort = "BEL" },
                new Country { CountryId = 2, CountryNameLong = "Netherlands", CountryNameShort = "NED" }
            };

            _countryServiceMock.Setup(s => s.GetAll()).ReturnsAsync(countries);
            _competitorServiceMock.Setup(s => s.GetCompetitorsByCountry(It.IsAny<int>()))
                .ReturnsAsync(5);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CountryViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.Contains(model, m => m.Name == "Belgium");
            Assert.Contains(model, m => m.CompetitorsCount == 5);
        }

        // =====================
        // DETAILS
        // =====================
        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            _countryServiceMock.Setup(s => s.GetById(It.IsAny<int>()))
                .ReturnsAsync((Country)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithCountry()
        {
            var country = new Country { CountryId = 1, CountryNameLong = "Belgium", CountryNameShort = "BEL" };
            _countryServiceMock.Setup(s => s.GetById(1)).ReturnsAsync(country);

            var result = await _controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(country, view.Model);
        }

        // =====================
        // CREATE (GET)
        // =====================
        [Fact]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        // =====================
        // CREATE (POST)
        // =====================
        [Fact]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            var country = new Country { CountryId = 1, CountryNameLong = "Belgium", CountryNameShort = "BEL" };

            var result = await _controller.Create(country);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _countryServiceMock.Verify(s => s.Create(country), Times.Once);
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsView()
        {
            var country = new Country();
            _controller.ModelState.AddModelError("error", "invalid");

            var result = await _controller.Create(country);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(country, view.Model);
        }

        // =====================
        // EDIT (GET)
        // =====================
        [Fact]
        public async Task Edit_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            _countryServiceMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync((Country)null);
            var result = await _controller.Edit(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ValidId_ReturnsViewWithCountry()
        {
            var country = new Country { CountryId = 1, CountryNameLong = "Belgium" };
            _countryServiceMock.Setup(s => s.GetById(1)).ReturnsAsync(country);

            var result = await _controller.Edit(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(country, view.Model);
        }

        // =====================
        // EDIT (POST)
        // =====================
        [Fact]
        public async Task Edit_IdMismatch_ReturnsNotFound()
        {
            var country = new Country { CountryId = 2 };
            var result = await _controller.Edit(1, country);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ValidModel_RedirectsToIndex()
        {
            var country = new Country { CountryId = 1, CountryNameLong = "Belgium" };
            var result = await _controller.Edit(1, country);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _countryServiceMock.Verify(s => s.Update(country), Times.Once);
        }

        [Fact]
        public async Task Edit_InvalidModel_ReturnsView()
        {
            var country = new Country { CountryId = 1 };
            _controller.ModelState.AddModelError("error", "invalid");

            var result = await _controller.Edit(1, country);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(country, view.Model);
        }

        [Fact]
        public async Task Edit_ConcurrencyException_CountryNotExists_ReturnsNotFound()
        {
            var country = new Country { CountryId = 1 };
            _countryServiceMock.Setup(s => s.Update(It.IsAny<Country>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());
            _countryServiceMock.Setup(s => s.GetById(1))
                .ReturnsAsync((Country)null);

            var result = await _controller.Edit(1, country);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ConcurrencyException_CountryStillExists_Throws()
        {
            var country = new Country { CountryId = 1 };
            _countryServiceMock.Setup(s => s.Update(It.IsAny<Country>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());
            _countryServiceMock.Setup(s => s.GetById(1))
                .ReturnsAsync(new Country { CountryId = 1 });

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _controller.Edit(1, country));
        }

        // =====================
        // DELETE
        // =====================
        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsNotFound()
        {
            _countryServiceMock.Setup(s => s.GetById(It.IsAny<int>())).ReturnsAsync((Country)null);
            var result = await _controller.Delete(99);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsViewWithCountry()
        {
            var country = new Country { CountryId = 1 };
            _countryServiceMock.Setup(s => s.GetById(1)).ReturnsAsync(country);

            var result = await _controller.Delete(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(country, view.Model);
        }

        // =====================
        // DELETE CONFIRMED
        // =====================
        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToIndex()
        {
            var country = new Country { CountryId = 1 };
            _countryServiceMock.Setup(s => s.GetById(1)).ReturnsAsync(country);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _countryServiceMock.Verify(s => s.Delete(country), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_CountryNotExists_RedirectsToIndexWithoutDelete()
        {
            _countryServiceMock.Setup(s => s.GetById(1)).ReturnsAsync((Country)null);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _countryServiceMock.Verify(s => s.Delete(It.IsAny<Country>()), Times.Never);
        }
    }
}
