using Microsoft.Playwright;

namespace CycleManager.Tests.E2E
{
    [TestClass]
    public class EventTests
    {
        private static AppFixture _fixture = null!;

        [ClassInitialize]
        public static async Task ClassSetup(TestContext context)
        {
            _fixture = new AppFixture();
            await _fixture.InitializeAsync();
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await _fixture.DisposeAsync();
        }

        [TestMethod]
        public async Task EventResultPage_Works_Correctly()
        {
            await _fixture.EnsureRunningAsync();
            var context = await _fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Open de homepage als fakeuser
            await page.GotoAsync($"{_fixture.WebBaseUrl}/?fakeuser=testuser", new() { Timeout = 60000 });

            // Wacht tot spinner verdwijnt
            await page.Locator(".loading-spinner.active").WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

            // Klik op “Tour de Test”
            var eventLink = page.GetByText("Tour de Test");
            await eventLink.ClickAsync();

            // Wacht tot de etappes geladen zijn
            var stageLink = page.Locator("a[href*='/Etappe?nummer=Proloog']");
            await Assertions.Expect(stageLink).ToBeVisibleAsync(new() { Timeout = 10000 });

            await stageLink.ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/Etappe.*"));

            var titleLocator = page.Locator("#etappe-title");
            await Assertions.Expect(titleLocator)
                .ToContainTextAsync("Etappe Proloog – Resultaten");

            // Controleer dat er uitslagen of resultaten zijn
            var resultsTable = page.Locator("#renner-lijst tr");
            await resultsTable.First.WaitForAsync(new() { Timeout = 10000 });

            // Controleer dat de tabel er is en resultaten bevat
            await Assertions.Expect(page.Locator("#renner-lijst td")).Not.ToHaveTextAsync("Geen resultaten");

            // Controleer dat de 'Terug'-knop zichtbaar is
            var terugLink = page.Locator("a#terug-naar-event");
            await Assertions.Expect(terugLink).ToBeVisibleAsync();

            // Klik op 'Terug'
            await terugLink.ClickAsync();

            // Controleer dat je weer op de Event-pagina bent
            await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/Event\\?eventId=1"));

            await context.CloseAsync();
        }

    }
}
