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
        #endregion
    }
}
