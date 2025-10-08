using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using CycleManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using WebCycleApi.Controllers;

namespace CycleManager.Tests.Unit.Api
{
    public class AccountControllerTests
    {
        private readonly AccountController _controller;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ITokenService> _mockTokenService;

        public AccountControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _mockEmailSender = new Mock<IEmailSender>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenService = new Mock<ITokenService>();

            _mockConfiguration.Setup(c => c["ClientSettings:FrontendBaseUrl"]).Returns("https://frontend.test");

            _controller = new AccountController(
                _mockUserManager.Object,
                _mockEmailSender.Object,
                _mockConfiguration.Object,
                _mockTokenService.Object
            );
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserCreatedAndEmailSent()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@testdomain.com",
                Password = "Password123!",
                FirstName = "Jonas",
                LastName = "Test"
            };

            var user = new ApplicationUser { Id = "1", Email = registerDto.Email, FirstName = "Jonas", LastName = "Test" };

            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync("token123");

            _mockEmailSender.Setup(e => e.SendEmailAsync(registerDto.Email, It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Registratie succesvol! Bevestig je e-mail.", okResult.Value);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserCreationFails()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "fail@testdomain.com",
                Password = "Password123!",
                FirstName = "Jonas",
                LastName = "Test"
            };

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Description = "Email bestaat al" }
            };

            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Contains("Email bestaat al", errorResponse.Errors);
        }

        [Fact]
        public async Task Register_Returns500_WhenEmailSendFails()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "failmail@testdomain.com",
                Password = "Password123!",
                FirstName = "Jonas",
                LastName = "Test"
            };

            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync("token123");

            _mockEmailSender.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .ThrowsAsync(new Exception("SMTP server offline"));

            // Act
            _controller.ModelState.Clear();
            var result = await _controller.Register(registerDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var errorResponse = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
            Assert.Contains("Fout bij verzenden van bevestigingsmail.", errorResponse.Errors);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var registerDto = new RegisterDto(); // leeg object

            _controller.ModelState.AddModelError("Email", "Email is verplicht");

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequest.Value;

            // Fallback-check, of controller een ErrorResponse of anonieme errors terugstuurt
            var errorsProp = value.GetType().GetProperty("Errors");
            Assert.NotNull(errorsProp);
            var errors = errorsProp.GetValue(value) as IEnumerable<string>;
            Assert.Contains("Email is verplicht", errors);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailDomainIsBlocked()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "hacker@example.com", // geblokkeerd domein
                Password = "Test123!",
                FirstName = "Evil",
                LastName = "User"
            };

            // Setup blocked domain check
            // Stel: onze controller (of service) zou dit valideren vóór UserManager.CreateAsync
            // daarom mocken we een foutresultaat terug
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Description = "E-mailadres niet toegestaan."
                }));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequest.Value;

            var errorsProp = value.GetType().GetProperty("Errors");
            Assert.NotNull(errorsProp);
            var errors = errorsProp.GetValue(value) as IEnumerable<string>;
            Assert.Contains("E-mailadres niet toegestaan.", errors);
        }

        [Fact]
        public async Task ConfirmEmail_NullUserIdOrToken_ReturnsBadRequest()
        {
            // Act
            var result1 = await _controller.ConfirmEmail(null, "token");
            var result2 = await _controller.ConfirmEmail("id", null);

            // Assert
            var badRequest1 = Assert.IsType<BadRequestObjectResult>(result1);
            Assert.Equal("Ongeldige bevestigingsgegevens.", badRequest1.Value);

            var badRequest2 = Assert.IsType<BadRequestObjectResult>(result2);
            Assert.Equal("Ongeldige bevestigingsgegevens.", badRequest2.Value);
        }

        [Fact]
        public async Task ConfirmEmail_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            string userId = "123";
            string token = "token123";

            _mockUserManager.Setup(u => u.FindByIdAsync(userId))
                            .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.ConfirmEmail(userId, token);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Gebruiker niet gevonden.", notFoundResult.Value);
        }

        [Fact]
        public async Task ConfirmEmail_Successful_RedirectsToFrontend()
        {
            // Arrange
            string userId = "123";
            string token = "token123";
            var user = new ApplicationUser { Id = userId, FirstName = "Theo", LastName = "de Tester" };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(u => u.ConfirmEmailAsync(user, token))
                            .ReturnsAsync(IdentityResult.Success);

            _mockConfiguration.Setup(c => c["ClientSettings:FrontendBaseUrl"])
                              .Returns("https://frontend.test");

            // Act
            var result = await _controller.ConfirmEmail(userId, token);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://frontend.test/Account/Bevestiging", redirectResult.Url);
        }

        [Fact]
        public async Task ConfirmEmail_FailedConfirmation_ReturnsBadRequest()
        {
            // Arrange
            string userId = "123";
            string token = "token123";
            var user = new ApplicationUser { Id = userId, FirstName = "Theo", LastName = "de Tester" };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(u => u.ConfirmEmailAsync(user, token))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed" }));

            // Act
            var result = await _controller.ConfirmEmail(userId, token);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("E-mail bevestiging mislukt.", badRequestResult.Value);
        }

        [Fact]
        public async Task ConfirmEmail_ExpiredToken_ReturnsBadRequest()
        {
            // Arrange
            string userId = "123";
            string token = "expiredToken";
            var user = new ApplicationUser {Id = userId, FirstName = "Theo", LastName = "de Tester" };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            // Simuleer een verlopen token (IdentityResult.Failed)
            _mockUserManager.Setup(u => u.ConfirmEmailAsync(user, token))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Token expired" }));

            // Act
            var result = await _controller.ConfirmEmail(userId, token);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("E-mail bevestiging mislukt.", badRequestResult.Value);
        }

        [Fact]
        public async Task ConfirmEmail_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            string userId = "123";
            string token = "invalidToken";
            var user = new ApplicationUser {Id = userId, FirstName = "Theo", LastName = "de Tester" };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId))
                            .ReturnsAsync(user);

            // Simuleer een ongeldig/malicious token
            _mockUserManager.Setup(u => u.ConfirmEmailAsync(user, token))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

            // Act
            var result = await _controller.ConfirmEmail(userId, token);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("E-mail bevestiging mislukt.", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithToken_WhenCredentialsAreValidAndEmailConfirmed()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
            var user = new ApplicationUser {Id = "user1", Email = loginDto.Email, FirstName = "Some", LastName = "One" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, loginDto.Password))
                            .ReturnsAsync(true);
            _mockUserManager.Setup(u => u.IsEmailConfirmedAsync(user))
                            .ReturnsAsync(true);
            _mockTokenService.Setup(t => t.CreateToken(user))
                             .Returns("fake-jwt-token");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.Equal("fake-jwt-token", data.Token);
            Assert.Equal("user1", data.UserId);
        }

        [Theory]
        [InlineData("wrong@example.com", "Password123!")]
        [InlineData("test@example.com", "WrongPassword")]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid(string email, string password)
        {
            // Arrange
            var loginDto = new LoginDto { Email = email, Password = password };
            _mockUserManager.Setup(u => u.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenEmailNotConfirmed()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
            var user = new ApplicationUser {Id = "user1", Email = loginDto.Email, FirstName = "Some", LastName = "One" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, loginDto.Password))
                            .ReturnsAsync(true);
            _mockUserManager.Setup(u => u.IsEmailConfirmedAsync(user))
                            .ReturnsAsync(false);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Bevestig eerst je e-mailadres.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "", Password = "" }; // leeg
            _controller.ModelState.AddModelError("Email", "Email is required");
            _controller.ModelState.AddModelError("Password", "Password is required");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<SerializableError>(badRequest.Value);
            Assert.True(errors.ContainsKey("Email"));
            Assert.True(errors.ContainsKey("Password"));
        }

        [Fact]
        public async Task ForgotPassword_ReturnsOk_WhenEmailSentSuccessfully()
        {
            // Arrange
            var dto = new ForgotPasswordDto { Email = "user@example.com" };
            var user = new ApplicationUser {Email = dto.Email, Id = "123", FirstName = "Some", LastName = "One" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(u => u.GeneratePasswordResetTokenAsync(user))
                            .ReturnsAsync("token123");

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockEmailSender.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsOk_WhenUserNotFound()
        {
            // Arrange
            var dto = new ForgotPasswordDto { Email = "unknown@example.com" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockEmailSender.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_Returns500_WhenEmailSendFails()
        {
            // Arrange
            var dto = new ForgotPasswordDto { Email = "error@example.com" };
            var user = new ApplicationUser {Email = dto.Email, Id = "456", FirstName = "Some", LastName = "One" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(u => u.GeneratePasswordResetTokenAsync(user))
                            .ReturnsAsync("token456");

            _mockEmailSender.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .ThrowsAsync(new Exception("SMTP offline"));

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);

            var error = Assert.IsType<ErrorResponse>(statusResult.Value);
            Assert.Contains("Fout bij verzenden van de resetmail.", error.Errors);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var dto = new ForgotPasswordDto { Email = "" };
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<SerializableError>(badRequest.Value);
            Assert.True(errors.ContainsKey("Email"));
        }

        [Fact]
        public async Task ResetPassword_ReturnsOk_WhenPasswordResetSucceeds()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                Email = "user@example.com",
                Password = "NewPassword123!",
                Token = "valid-token"
            };

            var user = new ApplicationUser {Email = dto.Email, Id = "1", FirstName = "Some", LastName = "One" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(u => u.ResetPasswordAsync(user, dto.Token, dto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenUserNotFound()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                Email = "notfound@example.com",
                Password = "Password123!",
                Token = "token"
            };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequest.Value);
            Assert.Contains("Gebruiker niet gevonden.", errorResponse.Errors);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenResetFails()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                Email = "user@example.com",
                Password = "NewPassword123!",
                Token = "invalid-token"
            };

            var user = new ApplicationUser {Email = dto.Email, Id = "2", FirstName = "Some", LastName = "One" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(user);

            var identityErrors = new List<IdentityError> 
            {
                new IdentityError { Description = "Invalid token" }, 
                new IdentityError { Description = "Password too weak" } 
            };

            _mockUserManager.Setup(u => u.ResetPasswordAsync(user, dto.Token, dto.Password))
                            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequest.Value);
            Assert.Contains("Invalid token", errorResponse.Errors);
            Assert.Contains("Password too weak", errorResponse.Errors);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var dto = new ResetPasswordDto { Email = "", Password = "", Token = "" };
            _controller.ModelState.AddModelError("Email", "Email is required");
            _controller.ModelState.AddModelError("Password", "Password is required");

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequest.Value);
            Assert.Contains("Email is required", errorResponse.Errors);
            Assert.Contains("Password is required", errorResponse.Errors);
        }

        [Fact]
        public async Task ResetPassword_Returns500_WhenExceptionThrown()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                Email = "user@example.com",
                Password = "NewPassword123!",
                Token = "valid-token"
            };

            var user = new ApplicationUser {Email = dto.Email, Id = "3", FirstName = "Some", LastName = "One" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(u => u.ResetPasswordAsync(user, dto.Token, dto.Password))
                            .ThrowsAsync(new Exception("Database timeout"));

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);

            var errorResponse = Assert.IsType<ErrorResponse>(objectResult.Value);
            Assert.Contains("Er is een fout opgetreden bij het resetten van het wachtwoord.", errorResponse.Errors);
            Assert.Contains("Database timeout", errorResponse.Errors);
        }
    }
}