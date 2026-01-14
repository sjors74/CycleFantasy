using CycleManager.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebCycleManager.Controllers;

namespace CycleManager.Tests.Unit.Manager
{
    public class NewsControllerTests
    {
        private readonly Mock<INewsService> _mockNewsService;
        private readonly NewsController _controller;

        public NewsControllerTests()
        {
            _mockNewsService = new Mock<INewsService>();
            _controller = new NewsController(_mockNewsService.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithNewsItems()
        {
            // Arrange
            var news = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Title1", Message = "Msg1", DatePosted = DateTime.Now, IsActive = true }
            };
            _mockNewsService.Setup(s => s.GetAllActiveNewsItems()).ReturnsAsync(news);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<NewsItem>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Title1", model[0].Title);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithNewNewsItem()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<NewsItem>(viewResult.Model);
            Assert.True(model.IsActive);
            Assert.NotEqual(default, model.DatePosted);
        }

        [Fact]
        public async Task Create_Post_ValidModel_CallsServiceAndRedirects()
        {
            var newsItem = new NewsItem { Title = "Test", Message = "Msg", DatePosted = DateTime.Now, IsActive = true };

            var result = await _controller.Create(newsItem);

            _mockNewsService.Verify(s => s.CreateAsync(newsItem), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Error", "Invalid");
            var newsItem = new NewsItem { Title = "test", Message = "test" };

            var result = await _controller.Create(newsItem);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(newsItem, view.Model);
        }

        [Fact]
        public async Task Edit_Get_InvalidId_ReturnsNotFound()
        {
            _mockNewsService.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((NewsItem)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            var news = new NewsItem { Id = 1, Title = "T", Message = "M", DatePosted = DateTime.Now, IsActive = true };
            _mockNewsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(news);

            var result = await _controller.Edit(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<NewsItem>(view.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Error", "Invalid");
            var newsItem = new NewsItem { Message = "test", Title = "test", Id = 1 };

            var result = await _controller.Edit(1, newsItem);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(newsItem, view.Model);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_CallsUpdateAndRedirects()
        {
            var existing = new NewsItem { Id = 1, Title = "Old", Message = "OldMsg", DatePosted = DateTime.Now, IsActive = true };
            var updated = new NewsItem { Id = 1, Title = "New", Message = "NewMsg", DatePosted = DateTime.Now, IsActive = false };

            _mockNewsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(existing);

            var result = await _controller.Edit(1, updated);

            _mockNewsService.Verify(s => s.UpdateAsync(existing), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_IdMismatch_ReturnsNotFound()
        {
            var updated = new NewsItem { Id = 2, Title = "test", Message = "test" };
            var result = await _controller.Edit(1, updated);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ThrowsConcurrency_ReturnsNotFound()
        {
            // Arrange
            var updated = new NewsItem { Id = 1, Title = "Test", Message = "Msg" };
            var existing = new NewsItem { Id = 1, Title = "Old", Message = "OldMsg" };

            _mockNewsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(existing);
            _mockNewsService.Setup(s => s.UpdateAsync(existing))
                .ThrowsAsync(new DbUpdateConcurrencyException());
            _mockNewsService.Setup(s => s.ExistsAsync(1)).ReturnsAsync(false);

            // Act
            var result = await _controller.Edit(1, updated);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ThrowsConcurrency_RethrowsException()
        {
            var updated = new NewsItem { Id = 1, Title = "Test", Message = "Msg" };
            var existing = new NewsItem { Id = 1, Title = "Old", Message = "OldMsg" };

            _mockNewsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(existing);
            _mockNewsService.Setup(s => s.UpdateAsync(existing))
                .ThrowsAsync(new DbUpdateConcurrencyException());
            _mockNewsService.Setup(s => s.ExistsAsync(1)).ReturnsAsync(true);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                _controller.Edit(1, updated));
        }

        [Fact]
        public async Task Delete_Get_InvalidId_ReturnsNotFound()
        {
            _mockNewsService.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((NewsItem)null);

            var result = await _controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ValidId_ReturnsView()
        {
            var news = new NewsItem { Id = 1, Title = "T", Message = "M" };
            _mockNewsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(news);

            var result = await _controller.Delete(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(news, view.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesAndRedirects()
        {
            var news = new NewsItem { Id = 1, Message = "test", Title = "test" };
            _mockNewsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(news);

            var result = await _controller.DeleteConfirmed(1);

            _mockNewsService.Verify(s => s.DeleteAsync(1), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_ItemNotFound_DoesNotCallDelete()
        {
            _mockNewsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((NewsItem)null);

            var result = await _controller.DeleteConfirmed(1);

            _mockNewsService.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
