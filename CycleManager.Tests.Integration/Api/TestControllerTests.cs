using Domain.Context;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCycle.Controllers;

namespace CycleManager.Tests.Integration.Api
{
    public class TestControllerTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void DumpEvents_ReturnsOkWithEvents()
        {
            using var db = CreateContext();

            db.Events.Add(new Event
            {
                EventId = 1,
                EventName = "Test Event"
            });

            db.SaveChanges();

            var controller = new TestController(db);

            var result = controller.DumpEvents();

            var okResult = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var events = okResult.Value.Should()
                .BeAssignableTo<List<Event>>()
                .Subject;

            events.Should().ContainSingle();
            events[0].EventName.Should().Be("Test Event");
        }

        [Fact]
        public async Task SeedReady_ReturnsOk()
        {
            await using var db = CreateContext();

            var controller = new TestController(db);

            var result = await controller.SeedReady();

            result.Should().BeOfType<OkResult>();

            db.Events.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetDeelnemers_ReturnsOkWithDeelnemers()
        {
            await using var db = CreateContext();

            db.GameCompetitorsEvent.Add(new GameCompetitorEvent
            {
                Id = 1,
                EventId = 1,
                UserId = "testuser",
                TeamName = "Mijn Pool"
            });

            await db.SaveChangesAsync();

            var controller = new TestController(db);

            var result = await controller.GetDeelnemers();

            var okResult = result.Should()
                .BeOfType<OkObjectResult>()
                .Subject;

            var deelnemers = okResult.Value
                .Should()
                .BeAssignableTo<List<GameCompetitorEvent>>()
                .Subject;

            deelnemers.Should().ContainSingle();
            deelnemers[0].TeamName.Should().Be("Mijn Pool");
        }
    }
}