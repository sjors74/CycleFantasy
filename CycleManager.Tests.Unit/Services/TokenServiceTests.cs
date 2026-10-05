using CycleManager.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace CycleManager.Tests.Unit.Services
{
    public class TokenServiceTests
    {
        [Fact]
        public void CreateToken_ValidUser_ReturnsValidJwt()
        {
            var config = CreateConfiguration();
            var service = new TokenService(config);

            var user = new IdentityUser
            {
                Id = "user-123",
                Email = "test@example.com"
            };

            var token = service.CreateToken(user);

            token.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            jwt.Issuer.Should().Be("test-issuer");
            jwt.Audiences.Should().Contain("test-audience");

            jwt.Claims.Should().ContainSingle(
                c => c.Type == JwtRegisteredClaimNames.Sub &&
                     c.Value == "user-123");

            jwt.Claims.Should().ContainSingle(
                c => c.Type == JwtRegisteredClaimNames.Email &&
                     c.Value == "test@example.com");

            jwt.Claims.Should().ContainSingle(
                c => c.Type == JwtRegisteredClaimNames.Jti &&
                     !string.IsNullOrWhiteSpace(c.Value));

            jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public void CreateToken_UserWithoutEmail_ThrowsInvalidOperationException()
        {
            var config = CreateConfiguration();
            var service = new TokenService(config);

            var user = new IdentityUser
            {
                Id = "user-123",
                Email = null
            };

            var act = () => service.CreateToken(user);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Gebruiker user-123 heeft geen e-mailadres.");
        }

        [Fact]
        public void CreateToken_EmptyEmail_ThrowsInvalidOperationException()
        {
            var config = CreateConfiguration();
            var service = new TokenService(config);

            var user = new IdentityUser
            {
                Id = "user-123",
                Email = "   "
            };

            var act = () => service.CreateToken(user);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Gebruiker user-123 heeft geen e-mailadres.");
        }

        [Fact]
        public void CreateToken_MissingJwtKey_ThrowsInvalidOperationException()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience"
                })
                .Build();

            var service = new TokenService(config);

            var user = new IdentityUser
            {
                Id = "user-123",
                Email = "test@example.com"
            };

            var act = () => service.CreateToken(user);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Jwt:Key is niet geconfigureerd.");
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "this-is-a-test-key-that-is-long-enough",
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience"
                })
                .Build();
        }
    }
}