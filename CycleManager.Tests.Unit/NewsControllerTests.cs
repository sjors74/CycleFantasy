using AutoMapper;
using CycleManager.Domain.Dto;
using CycleManager.Services.Interfaces;
using Domain.Models;
using Moq;
using WebCycleApi.Controllers;

namespace CycleManager.Tests.Unit
{
    public class NewsControllerTests
    {
        [Fact]
        public async Task GetAllNews_ReturnsMappedDtos_WhenNewsExists()
        {
            // Arrange
            var mockNewsService = new Mock<INewsService>();
            var mockMapper = new Mock<IMapper>();

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Breaking News", Message = "This is breaking news" },
                new NewsItem { Id = 2, Title = "Another Story", Message = "This is another story" }
            };

            var mappedDtos = new List<NewsItemDto>
            {
                new NewsItemDto { Id = 1, Title = "Breaking News", Message = "This is breaking news" },
                new NewsItemDto { Id = 2, Title = "Another Story", Message = "This is another story" }
            };

            mockNewsService.Setup(s => s.GetAllActiveNewsItems()).ReturnsAsync(newsItems);
            mockMapper.Setup(m => m.Map<List<NewsItemDto>>(newsItems)).Returns(mappedDtos);

            var controller = new NewsController(mockNewsService.Object, mockMapper.Object);

            // Act
            var result = await controller.GetAllNews();

            // Assert
            Assert.NotNull(result);
            var list = Assert.IsAssignableFrom<IEnumerable<NewsItemDto>>(result);
            Assert.Equal(2, ((List<NewsItemDto>)list).Count);
        }

        [Fact]
        public async Task GetAllNews_ReturnsEmptyList_WhenNoNewsExists()
        {
            // Arrange
            var mockNewsService = new Mock<INewsService>();
            var mockMapper = new Mock<IMapper>();

            mockNewsService.Setup(s => s.GetAllActiveNewsItems())
                           .ReturnsAsync(new List<NewsItem>());

            mockMapper.Setup(m => m.Map<List<NewsItemDto>>(It.IsAny<List<NewsItem>>()))
                      .Returns(new List<NewsItemDto>());

            var controller = new NewsController(mockNewsService.Object, mockMapper.Object);

            // Act
            var result = await controller.GetAllNews();

            // Assert
            var list = Assert.IsAssignableFrom<IEnumerable<NewsItemDto>>(result);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetAllNews_PropagatesException_WhenServiceThrows()
        {
            // Arrange
            var mockNewsService = new Mock<INewsService>();
            var mockMapper = new Mock<IMapper>();

            mockNewsService.Setup(s => s.GetAllActiveNewsItems())
                           .ThrowsAsync(new System.Exception("Database down"));

            var controller = new NewsController(mockNewsService.Object, mockMapper.Object);

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => controller.GetAllNews());
        }
    }
}