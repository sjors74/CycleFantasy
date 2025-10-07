using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebCycle.Controllers;

namespace CycleManager.Tests.Unit
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

            var controller = new ConfigController(configuration);

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
            var controller = new ConfigController(configuration);

            // Act
            var result = controller.Ping();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("pong", okResult.Value);
        }
    }

}
