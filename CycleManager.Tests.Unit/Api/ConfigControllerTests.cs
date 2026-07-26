using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit.Api
{
    public class ConfigControllerTests
    {
        [Fact]
        public void GetClientSettings_ReturnsApiBaseUrl_FromConfiguration()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"ClientSettings:ApiBaseUrl", "https://api.example.com"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var seasonYearServiceMock = new Mock<ISeasonYearService>();
            var controller = new ConfigController(configuration, seasonYearServiceMock.Object);

            // Act
            var result = controller.GetClientSettings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            // Gebruik reflection om het veld "apiBaseUrl" te lezen
            var property = value!.GetType().GetProperty("apiBaseUrl");
            var apiBaseUrlValue = property!.GetValue(value)?.ToString();

            Assert.Equal("https://api.example.com", apiBaseUrlValue);
        }


        [Fact]
        public void Ping_ReturnsPong()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build(); // lege config
            var seasonYearServiceMock = new Mock<ISeasonYearService>();
            var controller = new ConfigController(configuration, seasonYearServiceMock.Object);

            // Act
            var result = controller.Ping();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("pong", okResult.Value);
        }
    }
}
