using CycleManager.Domain.Enums;
using CycleManager.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Moq;

namespace CycleManager.Tests.Unit.Services
{
    public class PcsScraperTests
    {
        private readonly Mock<IBrowser> _browserMock;
        private readonly Mock<IBrowserContext> _contextMock;
        private readonly Mock<IPage> _pageMock;
        private readonly Mock<ILogger<PcsScraper>> _loggerMock;
        private readonly PcsScraper _service;

        public PcsScraperTests()
        {
            _browserMock = new Mock<IBrowser>();
            _contextMock = new Mock<IBrowserContext>();
            _pageMock = new Mock<IPage>();
            _loggerMock = new Mock<ILogger<PcsScraper>>();

            _browserMock
                .Setup(x => x.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
                .ReturnsAsync(_contextMock.Object);

            _contextMock
                .Setup(x => x.NewPageAsync())
                .ReturnsAsync(_pageMock.Object);

            _service = new PcsScraper(
                _loggerMock.Object,
                _browserMock.Object);
        }

        [Fact]
        public async Task ScrapeDropoutBibsAsync_WhenPageContainsDropouts_ReturnsBibNumbers()
        {
            var bib1 = new Mock<IElementHandle>();
            var bib2 = new Mock<IElementHandle>();

            bib1
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("12");

            bib2
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("34");

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync("li.dropout span.bib"))
                .ReturnsAsync(new[]
                {
            bib1.Object,
            bib2.Object
                });

            var result = await _service.ScrapeDropoutBibsAsync(
                "https://example.com/startlist");

            result.Should().BeEquivalentTo(new[] { 12, 34 });

            _pageMock.Verify(x => x.CloseAsync(), Times.Once);
            _contextMock.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ScrapeDropoutBibsAsync_WhenBibIsInvalid_SkipsInvalidBib()
        {
            var validBib = new Mock<IElementHandle>();
            var invalidBib = new Mock<IElementHandle>();

            validBib
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("12");

            invalidBib
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("abc");

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync("li.dropout span.bib"))
                .ReturnsAsync(new[]
                {
            validBib.Object,
            invalidBib.Object
                });

            var result = await _service.ScrapeDropoutBibsAsync(
                "https://example.com/startlist");

            result.Should().BeEquivalentTo(new[] { 12 });
        }

        [Fact]
        public async Task ScrapeDropoutBibsAsync_WhenStartlistTimesOut_ReturnsEmptyList()
        {
            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ThrowsAsync(new TimeoutException());

            var result = await _service.ScrapeDropoutBibsAsync(
                "https://example.com/startlist");

            result.Should().BeEmpty();

            _pageMock.Verify(x => x.CloseAsync(), Times.Once);
            _contextMock.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ScrapeDropoutBibsAsync_WhenScrapingThrows_ReturnsEmptyList()
        {
            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var result = await _service.ScrapeDropoutBibsAsync(
                "https://example.com/startlist");

            result.Should().BeEmpty();

            _pageMock.Verify(x => x.CloseAsync(), Times.Once);
            _contextMock.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenRidersAreFound_ReturnsCompetitors()
        {
            var rider1 = new Mock<IElementHandle>();
            var rider2 = new Mock<IElementHandle>();

            var name1 = new Mock<IElementHandle>();
            var name2 = new Mock<IElementHandle>();

            var flag1 = new Mock<IElementHandle>();
            var flag2 = new Mock<IElementHandle>();

            name1
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            name2
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Pogacar Tadej");

            flag1
                .Setup(x => x.GetAttributeAsync("class"))
                .ReturnsAsync("flag nl");

            flag2
                .Setup(x => x.GetAttributeAsync("class"))
                .ReturnsAsync("flag si");

            rider1
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync(name1.Object);

            rider1
                .Setup(x => x.QuerySelectorAsync("span.flag"))
                .ReturnsAsync(flag1.Object);

            rider2
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync(name2.Object);

            rider2
                .Setup(x => x.QuerySelectorAsync("span.flag"))
                .ReturnsAsync(flag2.Object);

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(new[]
                {
                    rider1.Object,
                    rider2.Object
                });

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            result.Should().HaveCount(2);

            result[0].RiderName.Should().Be("Jansen Jan");
            result[0].TeamId.Should().Be(42);
            result[0].Year.Should().Be(2026);
            result[0].CountryShortName.Should().Be("nl");

            result[1].RiderName.Should().Be("Pogacar Tadej");
            result[1].CountryShortName.Should().Be("si");

            _pageMock.Verify(x => x.CloseAsync(), Times.Once);
            _contextMock.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenUrlHasQueryStringAndRiderHasNoFlag_UsesAmpersandAndEmptyCountry()
        {
            var rider = new Mock<IElementHandle>();
            var nameElement = new Mock<IElementHandle>();

            nameElement
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            rider
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync(nameElement.Object);

            rider
                .Setup(x => x.QuerySelectorAsync("span.flag"))
                .ReturnsAsync((IElementHandle?)null);

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?foo=bar&x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(new[] { rider.Object });

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team?foo=bar",
                42,
                2026);

            result.Should().ContainSingle();

            result[0].RiderName.Should().Be("Jansen Jan");
            result[0].TeamId.Should().Be(42);
            result[0].Year.Should().Be(2026);
            result[0].CountryShortName.Should().BeEmpty();

            _pageMock.Verify(x => x.CloseAsync(), Times.Once);
            _contextMock.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenRiderListIsNotInitiallyAvailable_ClicksNameTabAndRetries()
        {
            var rider = new Mock<IElementHandle>();
            var nameElement = new Mock<IElementHandle>();
            var flagElement = new Mock<IElementHandle>();
            var tab = new Mock<IElementHandle>();

            nameElement
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            flagElement
                .Setup(x => x.GetAttributeAsync("class"))
                .ReturnsAsync("flag nl");

            rider
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync(nameElement.Object);

            rider
                .Setup(x => x.QuerySelectorAsync("span.flag"))
                .ReturnsAsync(flagElement.Object);

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            var waitCount = 0;

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(() =>
                {
                    waitCount++;

                    if (waitCount == 1)
                        throw new PlaywrightException("Rider list not available yet.");

                    return Mock.Of<IElementHandle>();
                });

            _pageMock
                .Setup(x => x.QuerySelectorAsync(
                    "div.borderbox.w30.right .tabnav1 .snav a[href*='snav=name']"))
                .ReturnsAsync(tab.Object);

            tab
                .Setup(x => x.ClickAsync(It.IsAny<ElementHandleClickOptions>()))
                .Returns(Task.CompletedTask);

            _pageMock
                .Setup(x => x.WaitForLoadStateAsync(LoadState.NetworkIdle))
                .Returns(Task.CompletedTask);

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(new[] { rider.Object });

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            result.Should().ContainSingle();
            result[0].RiderName.Should().Be("Jansen Jan");
            result[0].CountryShortName.Should().Be("nl");

            tab.Verify(
                x => x.ClickAsync(It.IsAny<ElementHandleClickOptions>()),
                Times.Once);

            _pageMock.Verify(
                x => x.WaitForLoadStateAsync(LoadState.NetworkIdle),
                Times.Once);
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenRiderListAndNameTabAreMissing_ThrowsException()
        {
            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ThrowsAsync(new PlaywrightException("Rider list not found"));

            _pageMock
                .Setup(x => x.QuerySelectorAsync(
                    "div.borderbox.w30.right .tabnav1 .snav a[href*='snav=name']"))
                .ReturnsAsync((IElementHandle?)null);

            _pageMock
                .Setup(x => x.ContentAsync())
                .ReturnsAsync("<html></html>");

            _pageMock
                .Setup(x => x.ScreenshotAsync(It.IsAny<PageScreenshotOptions>()))
                .ReturnsAsync(new byte[0]);

            var act = () => _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("*Kon teamlist niet vinden*");

            _pageMock.Verify(
                x => x.CloseAsync(),
                Times.Once);

            _contextMock.Verify(
                x => x.CloseAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenRiderListIsEmpty_ReturnsEmptyList()
        {
            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(Array.Empty<IElementHandle>());

            _pageMock
                .Setup(x => x.ContentAsync())
                .ReturnsAsync("<html></html>");

            _pageMock
                .Setup(x => x.ScreenshotAsync(It.IsAny<PageScreenshotOptions>()))
                .ReturnsAsync(Array.Empty<byte>());

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            result.Should().BeEmpty();

            _pageMock.Verify(x => x.ContentAsync(), Times.Once);
            _pageMock.Verify(
                x => x.ScreenshotAsync(It.IsAny<PageScreenshotOptions>()),
                Times.Once);
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenRiderHasNoNameElement_SkipsRider()
        {
            var rider = new Mock<IElementHandle>();

            rider
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync((IElementHandle?)null);

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(new[] { rider.Object });

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenRiderNameIsEmpty_SkipsRider()
        {
            var rider = new Mock<IElementHandle>();
            var nameElement = new Mock<IElementHandle>();

            nameElement
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("   ");

            rider
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync(nameElement.Object);

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(new[] { rider.Object });

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenFlagHasNoCountryClass_ReturnsEmptyCountry()
        {
            var rider = new Mock<IElementHandle>();
            var nameElement = new Mock<IElementHandle>();
            var flagElement = new Mock<IElementHandle>();

            nameElement
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            flagElement
                .Setup(x => x.GetAttributeAsync("class"))
                .ReturnsAsync("flag");

            rider
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync(nameElement.Object);

            rider
                .Setup(x => x.QuerySelectorAsync("span.flag"))
                .ReturnsAsync(flagElement.Object);

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(new[] { rider.Object });

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            result.Should().ContainSingle();
            result[0].CountryShortName.Should().BeEmpty();
            result[0].RiderName.Should().Be("Jansen Jan");
        }

        [Fact]
        public async Task ScrapeCompetitorsAsync_WhenFlagHasNoClassAttribute_ReturnsEmptyCountry()
        {
            var rider = new Mock<IElementHandle>();
            var nameElement = new Mock<IElementHandle>();
            var flagElement = new Mock<IElementHandle>();

            nameElement
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            flagElement
                .Setup(x => x.GetAttributeAsync("class"))
                .ReturnsAsync((string?)null);

            rider
                .Setup(x => x.QuerySelectorAsync("a"))
                .ReturnsAsync(nameElement.Object);

            rider
                .Setup(x => x.QuerySelectorAsync("span.flag"))
                .ReturnsAsync(flagElement.Object);

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/team?x=1&snav=name",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.TitleAsync())
                .ReturnsAsync("Team riders");

            _pageMock
                .Setup(x => x.Locator("body").InnerTextAsync())
                .ReturnsAsync("Riders");

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "div.stab.name.riderlistcont ul.teamlist li",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.QuerySelectorAllAsync(
                    "div.stab.name.riderlistcont ul.teamlist li"))
                .ReturnsAsync(new[] { rider.Object });

            var result = await _service.ScrapeCompetitorsAsync(
                "https://example.com/team",
                42,
                2026);

            result.Should().ContainSingle();
            result[0].CountryShortName.Should().BeEmpty();
        }

        [Fact]
        public async Task ScrapeStageResultsAsync_WhenValidResultsAreFound_ReturnsTopNResults()
        {
            var rows = new Mock<ILocator>();
            var row = new Mock<ILocator>();
            var cols = new Mock<ILocator>();

            var position = new Mock<ILocator>();
            var bib = new Mock<ILocator>();
            var riderCell = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();
            var teamCell = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Locator("table.results tbody tr"))
                .Returns(rows.Object);

            rows
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            rows
                .Setup(x => x.Nth(0))
                .Returns(row.Object);

            row
                .Setup(x => x.Locator("td"))
                .Returns(cols.Object);

            cols
                .Setup(x => x.CountAsync())
                .ReturnsAsync(9);

            cols
                .Setup(x => x.Nth(0))
                .Returns(position.Object);

            cols
                .Setup(x => x.Nth(3))
                .Returns(bib.Object);

            cols
                .Setup(x => x.Nth(7))
                .Returns(riderCell.Object);

            cols
                .Setup(x => x.Nth(8))
                .Returns(teamCell.Object);

            position
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("1");

            bib
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("101");

            riderCell
                .Setup(x => x.Locator("a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Rider One");

            teamCell
                .Setup(x => x.Locator("a"))
                .Returns(teamLink.Object);

            teamLink
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            teamLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Team One");

            var result = await _service.ScrapeStageResultsAsync(
                "https://example.com/stage",
                1,
                42);

            result.Should().ContainSingle();

            result[0].EventId.Should().Be(42);
            result[0].Position.Should().Be(1);
            result[0].BibNumber.Should().Be(101);
            result[0].RiderName.Should().Be("Rider One");
            result[0].TeamName.Should().Be("Team One");

            _pageMock.Verify(x => x.CloseAsync(), Times.Once);
            _contextMock.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ScrapeStageResultsAsync_WhenRowHasTooFewColumns_SkipsRow()
        {
            var rows = new Mock<ILocator>();
            var row = new Mock<ILocator>();
            var cols = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Locator("table.results tbody tr"))
                .Returns(rows.Object);

            rows
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            rows
                .Setup(x => x.Nth(0))
                .Returns(row.Object);

            row
                .Setup(x => x.Locator("td"))
                .Returns(cols.Object);

            cols
                .Setup(x => x.CountAsync())
                .ReturnsAsync(4);

            var result = await _service.ScrapeStageResultsAsync(
                "https://example.com/stage",
                10,
                42);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ScrapeStageResultsAsync_WhenBibIsInvalid_UsesZero()
        {
            var rows = new Mock<ILocator>();
            var row = new Mock<ILocator>();
            var cols = new Mock<ILocator>();

            var position = new Mock<ILocator>();
            var bib = new Mock<ILocator>();
            var riderCell = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();
            var teamCell = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Locator("table.results tbody tr"))
                .Returns(rows.Object);

            rows.Setup(x => x.CountAsync()).ReturnsAsync(1);
            rows.Setup(x => x.Nth(0)).Returns(row.Object);

            row.Setup(x => x.Locator("td")).Returns(cols.Object);

            cols.Setup(x => x.CountAsync()).ReturnsAsync(9);
            cols.Setup(x => x.Nth(0)).Returns(position.Object);
            cols.Setup(x => x.Nth(3)).Returns(bib.Object);
            cols.Setup(x => x.Nth(7)).Returns(riderCell.Object);
            cols.Setup(x => x.Nth(8)).Returns(teamCell.Object);

            position
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("1");

            bib
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("DNS");

            riderCell
                .Setup(x => x.Locator("a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Rider One");

            teamCell
                .Setup(x => x.Locator("a"))
                .Returns(teamLink.Object);

            teamLink.Setup(x => x.CountAsync()).ReturnsAsync(1);

            teamLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Team One");

            var result = await _service.ScrapeStageResultsAsync(
                "https://example.com/stage",
                10,
                42);

            result.Should().ContainSingle();
            result[0].BibNumber.Should().Be(0);
        }

        [Fact]
        public async Task ScrapeStageResultsAsync_WhenRiderNameIsEmpty_SkipsRow()
        {
            var rows = new Mock<ILocator>();
            var row = new Mock<ILocator>();
            var cols = new Mock<ILocator>();

            var position = new Mock<ILocator>();
            var bib = new Mock<ILocator>();
            var riderCell = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Locator("table.results tbody tr"))
                .Returns(rows.Object);

            rows.Setup(x => x.CountAsync()).ReturnsAsync(1);
            rows.Setup(x => x.Nth(0)).Returns(row.Object);

            row.Setup(x => x.Locator("td")).Returns(cols.Object);

            cols.Setup(x => x.CountAsync()).ReturnsAsync(9);
            cols.Setup(x => x.Nth(0)).Returns(position.Object);
            cols.Setup(x => x.Nth(3)).Returns(bib.Object);
            cols.Setup(x => x.Nth(7)).Returns(riderCell.Object);

            position
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("1");

            bib
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("101");

            riderCell
                .Setup(x => x.Locator("a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("   ");

            var result = await _service.ScrapeStageResultsAsync(
                "https://example.com/stage",
                10,
                42);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ScrapeStageResultsAsync_WhenTeamHasNoLink_UsesTeamCellText()
        {
            var rows = new Mock<ILocator>();
            var row = new Mock<ILocator>();
            var cols = new Mock<ILocator>();

            var position = new Mock<ILocator>();
            var bib = new Mock<ILocator>();
            var riderCell = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();
            var teamCell = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Locator("table.results tbody tr"))
                .Returns(rows.Object);

            rows.Setup(x => x.CountAsync()).ReturnsAsync(1);
            rows.Setup(x => x.Nth(0)).Returns(row.Object);

            row.Setup(x => x.Locator("td")).Returns(cols.Object);

            cols.Setup(x => x.CountAsync()).ReturnsAsync(9);
            cols.Setup(x => x.Nth(0)).Returns(position.Object);
            cols.Setup(x => x.Nth(3)).Returns(bib.Object);
            cols.Setup(x => x.Nth(7)).Returns(riderCell.Object);
            cols.Setup(x => x.Nth(8)).Returns(teamCell.Object);

            position.Setup(x => x.InnerTextAsync()).ReturnsAsync("1");
            bib.Setup(x => x.InnerTextAsync()).ReturnsAsync("101");

            riderCell
                .Setup(x => x.Locator("a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Rider One");

            teamCell
                .Setup(x => x.Locator("a"))
                .Returns(teamLink.Object);

            teamLink
                .Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            teamCell
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Team One");

            var result = await _service.ScrapeStageResultsAsync(
                "https://example.com/stage",
                10,
                42);

            result.Should().ContainSingle();
            result[0].TeamName.Should().Be("Team One");
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenValidStartlistIsFound_ReturnsRiders()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            teams
                .Setup(x => x.Nth(0))
                .Returns(team.Object);

            team
                .Setup(x => x.Locator("a.team"))
                .Returns(teamLink.Object);

            teamLink
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            teamLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Team One");

            teamLink
                .Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync("team/team-one");

            team
                .Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riders
                .Setup(x => x.Nth(0))
                .Returns(rider.Object);

            rider
                .Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("101");

            rider
                .Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.IsVisibleAsync())
                .ReturnsAsync(true);

            riderLink
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            riderLink
                .Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync("rider/jansen-jan");

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().ContainSingle();

            result[0].RiderName.Should().Be("Jansen Jan");
            result[0].PcsName.Should().Be("jansen-jan");
            result[0].TeamName.Should().Be("Team One");
            result[0].TeamPcsName.Should().Be("team-one");
            result[0].BibNumber.Should().Be(101);

            _pageMock.Verify(x => x.CloseAsync(), Times.Once);
            _contextMock.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenTeamHasNoLink_StillProcessesRiders()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team
                .Setup(x => x.Locator("a.team"))
                .Returns(teamLink.Object);

            teamLink
                .Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            team
                .Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riders.Setup(x => x.Nth(0)).Returns(rider.Object);

            rider
                .Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("101");

            rider
                .Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync()).ReturnsAsync(true);
            riderLink.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riderLink.Setup(x => x.InnerTextAsync()).ReturnsAsync("Jansen Jan");
            riderLink.Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync("rider/jansen-jan");

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().ContainSingle();

            result[0].RiderName.Should().Be("Jansen Jan");
            result[0].PcsName.Should().Be("jansen-jan");
            result[0].TeamName.Should().BeEmpty();
            result[0].TeamPcsName.Should().BeEmpty();
            result[0].BibNumber.Should().Be(101);
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenRiderHasNoBib_ReturnsNullBib()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team")).Returns(teamLink.Object);
            teamLink.Setup(x => x.CountAsync()).ReturnsAsync(0);

            team
                .Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riders.Setup(x => x.Nth(0)).Returns(rider.Object);

            rider
                .Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            rider
                .Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync()).ReturnsAsync(true);
            riderLink.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riderLink.Setup(x => x.InnerTextAsync()).ReturnsAsync("Jansen Jan");
            riderLink
                .Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync("rider/jansen-jan");

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().ContainSingle();

            result[0].BibNumber.Should().BeNull();
            result[0].RiderName.Should().Be("Jansen Jan");
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenBibIsInvalid_ReturnsNullBib()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team")).Returns(teamLink.Object);
            teamLink.Setup(x => x.CountAsync()).ReturnsAsync(0);

            team
                .Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riders.Setup(x => x.Nth(0)).Returns(rider.Object);

            rider
                .Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator.Setup(x => x.CountAsync()).ReturnsAsync(1);
            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("abc");

            rider
                .Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync()).ReturnsAsync(true);
            riderLink.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riderLink.Setup(x => x.InnerTextAsync()).ReturnsAsync("Jansen Jan");
            riderLink
                .Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync("rider/jansen-jan");

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().ContainSingle();
            result[0].BibNumber.Should().BeNull();
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenRiderLinkIsNotVisible_SkipsRider()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team")).Returns(teamLink.Object);
            teamLink.Setup(x => x.CountAsync()).ReturnsAsync(0);

            team.Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riders.Setup(x => x.Nth(0)).Returns(rider.Object);

            rider.Setup(x => x.Locator(".bib")).Returns(bibLocator.Object);
            bibLocator.Setup(x => x.CountAsync()).ReturnsAsync(0);

            rider.Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.IsVisibleAsync())
                .ReturnsAsync(false);

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().BeEmpty();

            riderLink.Verify(
                x => x.CountAsync(),
                Times.Never);
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenRiderLinkCountIsZero_SkipsRider()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team")).Returns(teamLink.Object);
            teamLink.Setup(x => x.CountAsync()).ReturnsAsync(0);

            team.Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync()).ReturnsAsync(1);
            riders.Setup(x => x.Nth(0)).Returns(rider.Object);

            rider.Setup(x => x.Locator(".bib")).Returns(bibLocator.Object);
            bibLocator.Setup(x => x.CountAsync()).ReturnsAsync(0);

            rider.Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync()).ReturnsAsync(true);
            riderLink.Setup(x => x.CountAsync()).ReturnsAsync(0);

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().BeEmpty();

            riderLink.Verify(
                x => x.CountAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenRiderNameIsEmpty_SkipsRider()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team"))
                .Returns(teamLink.Object);

            teamLink.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            team.Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riders.Setup(x => x.Nth(0))
                .Returns(rider.Object);

            rider.Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            rider.Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync())
                .ReturnsAsync(true);

            riderLink.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riderLink.Setup(x => x.InnerTextAsync())
                .ReturnsAsync("   ");

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().BeEmpty();

            riderLink.Verify(
                x => x.GetAttributeAsync("href"),
                Times.Never);
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenRiderHrefIsMissing_UsesEmptyPcsName()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();
            var rider = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team"))
                .Returns(teamLink.Object);

            teamLink.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            team.Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riders.Setup(x => x.Nth(0))
                .Returns(rider.Object);

            rider.Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            rider.Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync())
                .ReturnsAsync(true);

            riderLink.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riderLink.Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            riderLink.Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync((string?)null);

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().ContainSingle();

            result[0].RiderName.Should().Be("Jansen Jan");
            result[0].PcsName.Should().BeEmpty();
            result[0].TeamName.Should().BeEmpty();
            result[0].TeamPcsName.Should().BeEmpty();
            result[0].BibNumber.Should().BeNull();
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenDuplicateRiderExists_LogsWarning()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();

            var rider1 = new Mock<ILocator>();
            var rider2 = new Mock<ILocator>();

            var bib1 = new Mock<ILocator>();
            var bib2 = new Mock<ILocator>();

            var riderLink1 = new Mock<ILocator>();
            var riderLink2 = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team"))
                .Returns(teamLink.Object);

            teamLink.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            teamLink.Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Team A");

            teamLink.Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync("team/team-a");

            team.Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync()).ReturnsAsync(2);
            riders.Setup(x => x.Nth(0)).Returns(rider1.Object);
            riders.Setup(x => x.Nth(1)).Returns(rider2.Object);

            SetupRider(rider1, bib1, riderLink1, "Jansen Jan", "rider/jansen-jan");
            SetupRider(rider2, bib2, riderLink2, "Jansen Jan", "rider/jansen-jan");

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().HaveCount(2);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) =>
                        state.ToString()!.Contains("Dubbele renner gevonden")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ScrapeStartlistAsync_WhenDuplicateBibExists_LogsWarning()
        {
            var teams = new Mock<ILocator>();
            var team = new Mock<ILocator>();
            var teamLink = new Mock<ILocator>();
            var riders = new Mock<ILocator>();

            var rider1 = new Mock<ILocator>();
            var rider2 = new Mock<ILocator>();

            var bib1 = new Mock<ILocator>();
            var bib2 = new Mock<ILocator>();

            var riderLink1 = new Mock<ILocator>();
            var riderLink2 = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/startlist",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.WaitForSelectorAsync(
                    "ul.startlist_v4",
                    It.IsAny<PageWaitForSelectorOptions>()))
                .ReturnsAsync(Mock.Of<IElementHandle>());

            _pageMock
                .Setup(x => x.Locator("ul.startlist_v4 > li"))
                .Returns(teams.Object);

            teams.Setup(x => x.CountAsync()).ReturnsAsync(1);
            teams.Setup(x => x.Nth(0)).Returns(team.Object);

            team.Setup(x => x.Locator("a.team"))
                .Returns(teamLink.Object);

            teamLink.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            teamLink.Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Team A");

            teamLink.Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync("team/team-a");

            team.Setup(x => x.Locator(".ridersCont > ul > li"))
                .Returns(riders.Object);

            riders.Setup(x => x.CountAsync()).ReturnsAsync(2);
            riders.Setup(x => x.Nth(0)).Returns(rider1.Object);
            riders.Setup(x => x.Nth(1)).Returns(rider2.Object);

            SetupRiderWithBib(
                rider1,
                bib1,
                riderLink1,
                "Jansen Jan",
                "rider/jansen-jan",
                "101");

            SetupRiderWithBib(
                rider2,
                bib2,
                riderLink2,
                "Pieters Piet",
                "rider/pieters-piet",
                "101");

            var result = await _service.ScrapeStartlistAsync(
                "https://example.com/startlist");

            result.Should().HaveCount(2);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) =>
                        state.ToString()!.Contains("Dubbel startnummer gevonden")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ScrapeClassificationWinnerFromUrlAsync_WhenWinnerIsFound_ReturnsResult()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage-1-gc",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            winnerRow
                .Setup(x => x.Locator("td.bibs"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("123");

            winnerRow
                .Setup(x => x.Locator("td.ridername a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            var result = await _service.ScrapeClassificationWinnerFromUrlAsync(
                "https://example.com/stage-1-gc",
                42,
                QuestionType.GC);

            result.Should().NotBeNull();
            result!.StageId.Should().Be(42);
            result.QuestionType.Should().Be(QuestionType.GC);
            result.BibNumber.Should().Be(123);
            result.RiderName.Should().Be("Jansen Jan");
        }

        [Fact]
        public async Task ScrapeClassificationWinnerFromUrlAsync_WhenNoVisibleResultsTable_ReturnsNull()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage-1-gc",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            var result = await _service.ScrapeClassificationWinnerFromUrlAsync(
                "https://example.com/stage-1-gc",
                42,
                QuestionType.GC);

            result.Should().BeNull();

            visibleTable.Verify(
                x => x.Locator("tbody tr"),
                Times.Never);
        }

        [Fact]
        public async Task ScrapeClassificationWinnerFromUrlAsync_WhenNoWinnerRow_ReturnsNull()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRows = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage-1-gc",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRows.Object);

            winnerRows
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            var result = await _service.ScrapeClassificationWinnerFromUrlAsync(
                "https://example.com/stage-1-gc",
                42,
                QuestionType.GC);

            result.Should().BeNull();

            winnerRow.Verify(
                x => x.Locator("td.bibs"),
                Times.Never);
        }

        [Fact]
        public async Task ScrapeClassificationWinnerFromUrlAsync_WhenBibIsInvalid_ReturnsNull()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRows = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage-1-gc",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRows.Object);

            winnerRows
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            winnerRow
                .Setup(x => x.Locator("td.bibs"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("geen-bib");

            var result = await _service.ScrapeClassificationWinnerFromUrlAsync(
                "https://example.com/stage-1-gc",
                42,
                QuestionType.GC);

            result.Should().BeNull();

            winnerRow.Verify(
                x => x.Locator("td.ridername a"),
                Times.Never);
        }

        [Fact]
        public async Task ScrapeClassificationWinnerAsync_WhenStageClassificationIsFound_ReturnsResult()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRows = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage-1-gc",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRows.Object);

            winnerRows
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            winnerRow
                .Setup(x => x.Locator("td.bibs"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("123");

            winnerRow
                .Setup(x => x.Locator("td.ridername a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            var result = await _service.ScrapeClassificationWinnerAsync(
                "https://example.com/stage-1",
                42,
                QuestionType.GC);

            result.Should().NotBeNull();
            result!.StageId.Should().Be(42);
            result.QuestionType.Should().Be(QuestionType.GC);
            result.BibNumber.Should().Be(123);
            result.RiderName.Should().Be("Jansen Jan");
        }

        [Fact]
        public async Task ScrapeClassificationWinnerAsync_WhenStageClassificationIsNotFound_UsesRaceFallback()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRows = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            var callCount = 0;

            _pageMock
                .Setup(x => x.GotoAsync(
                    It.IsAny<string>(),
                    It.IsAny<PageGotoOptions>()))
                .Callback<string, PageGotoOptions>((url, _) =>
                {
                    callCount++;

                    if (callCount == 1)
                        url.Should().Be("https://example.com/stage-1-gc");
                    else
                        url.Should().Be("https://example.com/gc");
                })
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .SetupSequence(x => x.CountAsync())
                .ReturnsAsync(0)
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRows.Object);

            winnerRows
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            winnerRow
                .Setup(x => x.Locator("td.bibs"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("123");

            winnerRow
                .Setup(x => x.Locator("td.ridername a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            var result = await _service.ScrapeClassificationWinnerAsync(
                "https://example.com/stage-1",
                42,
                QuestionType.GC);

            result.Should().NotBeNull();
            result!.StageId.Should().Be(42);
            result.QuestionType.Should().Be(QuestionType.GC);
            result.BibNumber.Should().Be(123);
            result.RiderName.Should().Be("Jansen Jan");

            callCount.Should().Be(2);
        }

        [Fact]
        public async Task ScrapeClassificationWinnerAsync_WhenStageUrlFailsWithPlaywrightException_UsesRaceFallback()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRows = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .SetupSequence(x => x.GotoAsync(
                    It.IsAny<string>(),
                    It.IsAny<PageGotoOptions>()))
                .ThrowsAsync(new PlaywrightException("Stage URL failed"))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRows.Object);

            winnerRows
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            winnerRow
                .Setup(x => x.Locator("td.bibs"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("123");

            winnerRow
                .Setup(x => x.Locator("td.ridername a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            var result = await _service.ScrapeClassificationWinnerAsync(
                "https://example.com/stage-1",
                42,
                QuestionType.GC);

            result.Should().NotBeNull();
            result!.StageId.Should().Be(42);
            result.QuestionType.Should().Be(QuestionType.GC);
            result.BibNumber.Should().Be(123);
            result.RiderName.Should().Be("Jansen Jan");
        }

        [Fact]
        public async Task ScrapeClassificationWinnerWithRetryAsync_WhenFirstAttemptSucceeds_ReturnsResult()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRows = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    "https://example.com/stage-1-gc",
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRows.Object);

            winnerRows
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            winnerRow
                .Setup(x => x.Locator("td.bibs"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("123");

            winnerRow
                .Setup(x => x.Locator("td.ridername a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            var result = await _service.ScrapeClassificationWinnerWithRetryAsync(
                "https://example.com/stage-1",
                42,
                QuestionType.GC);

            result.Should().NotBeNull();
            result!.StageId.Should().Be(42);
            result.QuestionType.Should().Be(QuestionType.GC);
            result.BibNumber.Should().Be(123);
            result.RiderName.Should().Be("Jansen Jan");
        }

        private static void SetupRider(
            Mock<ILocator> rider,
            Mock<ILocator> bibLocator,
            Mock<ILocator> riderLink,
            string riderName,
            string href)
        {
            rider.Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            rider.Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync())
                .ReturnsAsync(true);

            riderLink.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riderLink.Setup(x => x.InnerTextAsync())
                .ReturnsAsync(riderName);

            riderLink.Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync(href);
        }

        [Fact]
        public async Task ScrapeClassificationWinnerWithRetryAsync_WhenFirstAttemptFailsAndSecondSucceeds_ReturnsResult()
        {
            var tables = new Mock<ILocator>();
            var visibleTable = new Mock<ILocator>();
            var winnerRows = new Mock<ILocator>();
            var winnerRow = new Mock<ILocator>();
            var bibLocator = new Mock<ILocator>();
            var riderLink = new Mock<ILocator>();

            _pageMock
                .Setup(x => x.GotoAsync(
                    It.IsAny<string>(),
                    It.IsAny<PageGotoOptions>()))
                .ReturnsAsync(Mock.Of<IResponse>());

            _pageMock
                .Setup(x => x.Url)
                .Returns("https://example.com/stage-1-gc");

            _pageMock
                .Setup(x => x.Locator("table.results"))
                .Returns(tables.Object);

            tables
                .Setup(x => x.Filter(It.IsAny<LocatorFilterOptions>()))
                .Returns(visibleTable.Object);

            visibleTable
                .SetupSequence(x => x.CountAsync())
                .ReturnsAsync(0)
                .ReturnsAsync(1);

            visibleTable
                .Setup(x => x.Locator("tbody tr"))
                .Returns(winnerRows.Object);

            winnerRows
                .Setup(x => x.First)
                .Returns(winnerRow.Object);

            winnerRow
                .Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            winnerRow
                .Setup(x => x.Locator("td.bibs"))
                .Returns(bibLocator.Object);

            bibLocator
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("123");

            winnerRow
                .Setup(x => x.Locator("td.ridername a"))
                .Returns(riderLink.Object);

            riderLink
                .Setup(x => x.InnerTextAsync())
                .ReturnsAsync("Jansen Jan");

            var result = await _service.ScrapeClassificationWinnerWithRetryAsync(
                "https://example.com/stage-1",
                42,
                QuestionType.GC);

            result.Should().NotBeNull();
            result!.StageId.Should().Be(42);
            result.QuestionType.Should().Be(QuestionType.GC);
            result.BibNumber.Should().Be(123);
            result.RiderName.Should().Be("Jansen Jan");

            visibleTable.Verify(
                x => x.CountAsync(),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ScrapeClassificationWinnerAsync_WhenQuestionTypeIsUnsupported_ThrowsNotSupportedException()
        {
            var act = () => _service.ScrapeClassificationWinnerAsync(
                "https://example.com/stage-1",
                42,
                (QuestionType)999);

            await act.Should()
                .ThrowAsync<NotSupportedException>()
                .WithMessage("QuestionType '999' is not supported by PCS.");
        }

        private static void SetupRiderWithBib(
            Mock<ILocator> rider,
            Mock<ILocator> bibLocator,
            Mock<ILocator> riderLink,
            string riderName,
            string href,
            string bib)
        {
            rider.Setup(x => x.Locator(".bib"))
                .Returns(bibLocator.Object);

            bibLocator.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            bibLocator.Setup(x => x.InnerTextAsync())
                .ReturnsAsync(bib);

            rider.Setup(x => x.Locator("a[href^='rider/']"))
                .Returns(riderLink.Object);

            riderLink.Setup(x => x.IsVisibleAsync())
                .ReturnsAsync(true);

            riderLink.Setup(x => x.CountAsync())
                .ReturnsAsync(1);

            riderLink.Setup(x => x.InnerTextAsync())
                .ReturnsAsync(riderName);

            riderLink.Setup(x => x.GetAttributeAsync("href"))
                .ReturnsAsync(href);
        }
    }
}