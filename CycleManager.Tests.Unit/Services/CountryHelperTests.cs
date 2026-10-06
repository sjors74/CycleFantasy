using CycleManager.Services.Helpers;
using FluentAssertions;

namespace CycleManager.Tests.Unit.Services.Helpers
{
    public class CountryHelperTests
    {
        [Fact]
        public void GetName_WithKnownIsoCode_ReturnsCountryName()
        {
            // Act
            var result = CountryHelper.GetName("NL");

            // Assert
            result.Should().Be("Nederland");
        }

        [Fact]
        public void GetName_WithLowercaseIsoCode_ReturnsCountryName()
        {
            // Act
            var result = CountryHelper.GetName("nl");

            // Assert
            result.Should().Be("Nederland");
        }

        [Fact]
        public void GetName_WithUnknownIsoCode_ReturnsIsoCode()
        {
            // Act
            var result = CountryHelper.GetName("XX");

            // Assert
            result.Should().Be("XX");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void GetName_WithEmptyOrWhitespaceIsoCode_ReturnsEmptyString(string? isoCode)
        {
            // Act
            var result = CountryHelper.GetName(isoCode!);

            // Assert
            result.Should().BeEmpty();
        }
    }
}