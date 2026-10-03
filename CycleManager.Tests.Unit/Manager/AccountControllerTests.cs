using CycleManager.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using WebCycleManager.Controllers;

namespace CycleManager.Tests.Unit.Manager
{
    public class AccountControllerTests
    {
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();

            var userManager = new UserManager<ApplicationUser>(
                userStore.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!,
                null!,
                null!,
                null!);

            _controller = new AccountController(_signInManagerMock.Object);
        }
        #region Login tests

        [Fact]
        public void Login_Get_SetsReturnUrlAndReturnsView()
        {
            // Act
            var result = _controller.Login("/Home/Index");

            // Assert
            result.Should().BeOfType<ViewResult>();
            ((string)_controller.ViewBag.ReturnUrl).Should().Be("/Home/Index");
        }

        [Fact]
        public async Task Login_Post_RedirectsToHome_WhenLoginSucceedsWithoutReturnUrl()
        {
            // Arrange
            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    "user",
                    "password",
                    false,
                    true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login("user", "password");

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }

        [Fact]
        public async Task Login_Post_RedirectsToLocalReturnUrl_WhenLoginSucceeds()
        {
            // Arrange
            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    "user",
                    "password",
                    false,
                    true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper
                .Setup(x => x.IsLocalUrl("/Home/Index"))
                .Returns(true);

            _controller.Url = urlHelper.Object;

            // Act
            var result = await _controller.Login(
                "user",
                "password",
                "/Home/Index");

            // Assert
            var redirect = result.Should().BeOfType<RedirectResult>().Subject;

            redirect.Url.Should().Be("/Home/Index");
        }

        [Fact]
        public async Task Login_Post_ReturnsView_WhenLoginFails()
        {
            // Arrange
            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    "user",
                    "wrong-password",
                    false,
                    true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(
                "user",
                "wrong-password",
                "/Home/Index");

            // Assert
            result.Should().BeOfType<ViewResult>();

            _controller.ModelState.IsValid.Should().BeFalse();
            _controller.ModelState[string.Empty]!.Errors
                .Should()
                .ContainSingle()
                .Which.ErrorMessage
                .Should()
                .Be("Ongeldige gebruikersnaam of wachtwoord.");

            ((string)_controller.ViewBag.ReturnUrl)
                .Should()
                .Be("/Home/Index");
        }

        [Fact]
        public async Task Login_Post_RedirectsToHome_WhenReturnUrlIsNotLocal()
        {
            // Arrange
            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    "user",
                    "password",
                    false,
                    true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper
                .Setup(x => x.IsLocalUrl("https://evil.example.com"))
                .Returns(false);

            _controller.Url = urlHelper.Object;

            // Act
            var result = await _controller.Login(
                "user",
                "password",
                "https://evil.example.com");

            // Assert
            var redirect = result.Should()
                .BeOfType<RedirectToActionResult>()
                .Subject;

            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Home");
        }
        #endregion

        #region Logout tests

        [Fact]
        public async Task Logout_SignsOutAndRedirectsToLogin()
        {
            // Arrange
            _signInManagerMock
                .Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;
            redirect.ActionName.Should().Be("Login");

            _signInManagerMock.Verify(
                x => x.SignOutAsync(),
                Times.Once);
        }

        #endregion

        #region AccessDenied tests

        [Fact]
        public void AccessDenied_ReturnsView()
        {
            // Act
            var result = _controller.AccessDenied();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        #endregion
    }
}
