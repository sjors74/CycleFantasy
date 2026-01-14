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

            try
            {
                // Login als testuser
                await page.GotoAsync($"{_fixture.WebBaseUrl}/?fakeuser=testuser", new() { Timeout = 60000 });

                // Wacht tot spinner klaar is
                var spinner = page.Locator(".loading-spinner.active");
                await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

                // Klik eerste event ("Tour de Test")
                var eventTile = page.Locator(".tile.tile-event").First;
                await eventTile.ClickAsync();

                // Wacht tot eventpagina geladen is
                await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

                var title = page.Locator("h1");
                await Assertions.Expect(title).ToHaveTextAsync("E2E Test Event");

                // Klik op de etappe-link “1”
                var proloogLink = page.Locator("a:text-is('1')");
                await proloogLink.ClickAsync();

                // Wacht tot resultatenpagina geladen is
                await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

                var etappeTitle = page.Locator("h1#etappe-title");
                await Assertions.Expect(etappeTitle)
                    .ToHaveTextAsync("Etappe 1 – Resultaten", new() { Timeout = 10000 });

                // Controleer dat er resultaten zijn
                var rows = page.Locator("#renner-lijst tr");
                int rowCount = await rows.CountAsync();

                Assert.IsTrue(rowCount > 0, "Geen resultaten gevonden in de tabel!");

                // Klik op “← Terug”
                var terugLink = page.Locator("a#terug-naar-event");
                await terugLink.ClickAsync();

                await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

                // Controleer dat we terug zijn op de eventpagina
                var eventTitle = page.Locator("h1");
                await Assertions.Expect(eventTitle).ToContainTextAsync("E2E Test Event");

                Console.WriteLine("EventResultPage test succesvol afgerond!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test gefaald: " + ex.Message);

                // Maak screenshot bij fout
                var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "event_error.png");
                await page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                Console.WriteLine($"Screenshot opgeslagen: {screenshotPath}");

                var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "event_error.html");
                await File.WriteAllTextAsync(htmlPath, await page.ContentAsync());
                Console.WriteLine($"HTML dump opgeslagen: {htmlPath}");

                throw; // Laat de test nog steeds falen
            }
            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
            }
        }
    }
}
