using Microsoft.Playwright;

namespace CycleManager.Tests.E2E
{
    [TestClass]
    public class HomeTests
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
        public async Task Homepage_Has_MenuAndWelcomMessage()
        {
            await _fixture.EnsureRunningAsync();

            var context = await _fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync($"{_fixture.WebBaseUrl}", new() { Timeout = 60000 });

            // wacht tot spinner verdwijnt
            var spinner = page.Locator(".loading-spinner.active");
            await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

            // check op welkomsttekst
            var welcomeText = page.Locator("text=Welkom bij Tourmanager");
            Assert.IsTrue(await welcomeText.IsVisibleAsync(), "De welkomstekst is niet zichtbaar na laden van de pagina.");

            await context.CloseAsync();
        }

        [TestMethod]
        public async Task Homepage_Navigation_Works_Correctly()
        {
            await _fixture.EnsureRunningAsync();

            var context = await _fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync($"{_fixture.WebBaseUrl}/", new() { Timeout = 60000 });

            await page.Locator(".loading-spinner.active").WaitForAsync(new()
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 60000
            });

            await Assertions.Expect(page.Locator("text=Welkom bij Tourmanager")).ToBeVisibleAsync();

            //// Klik op 'Inloggen'
            var loginLink = page.GetByRole(AriaRole.Link, new() { Name = "Inloggen" });
            await loginLink.ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/Account/Login"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Inloggen" }))
                .ToBeVisibleAsync();

            // Klik op 'Registreren'
            var registerLink = page.GetByRole(AriaRole.Link, new() { Name = "Registreren" });
            await registerLink.ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/Account/Register"));
            await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Registreren" }))
                .ToBeVisibleAsync();

            // Klik op 'Home'
            var homeLink = page.GetByRole(AriaRole.Link, new() { Name = "Home" });
            await homeLink.ClickAsync();

            await Assertions.Expect(page).ToHaveURLAsync(new Regex(".*/$"));
            await Assertions.Expect(page.GetByText("Welkom bij Tourmanager")).ToBeVisibleAsync();

            await context.CloseAsync();
        }

        [TestMethod]
        public async Task Login_WithValidCredentials_Works()
        {
            await _fixture.EnsureRunningAsync();

            var context = await _fixture.Browser.NewContextAsync();

            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync("https://localhost:7089/?fakeuser=testuser");

                await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Profiel" }))
                    .ToBeVisibleAsync(new() { Timeout = 15000 });

                Console.WriteLine("Login test passed: 'Profiel' link is zichtbaar!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login test failed: " + ex.Message);

                var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "login_error.png");
                await page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                Console.WriteLine($"Screenshot saved at: {screenshotPath}");

                var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "login_error.html");
                await File.WriteAllTextAsync(htmlPath, await page.ContentAsync());
                Console.WriteLine($"HTML dump saved at: {htmlPath}");

                throw; // Gooi de fout door zodat de test nog steeds faalt
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [TestMethod]
        public async Task Homepage_Shows_LoginLink_WhenUserNotSignedIn()
        {
            await _fixture.EnsureRunningAsync();

            var context = await _fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Geen ?fakeuser= meegeven → user is NIET ingelogd
            await page.GotoAsync($"{_fixture.WebBaseUrl}/", new() { Timeout = 60000 });

            // wacht tot spinner verdwijnt (zoals bij je andere tests)
            var spinner = page.Locator(".loading-spinner.active");
            await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

            // 🔹 Controleer dat 'Inloggen' zichtbaar is
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Inloggen" }))
                .ToBeVisibleAsync(new() { Timeout = 10000 });

            // 🔹 Controleer dat 'Profiel' juist NIET zichtbaar is
            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Profiel" }))
                .Not.ToBeVisibleAsync();

            Console.WriteLine("Unsigned test passed: 'Inloggen' is zichtbaar, 'Profiel' niet.");

            await context.CloseAsync();
        }

        [TestMethod]
        public async Task LoggedInUser_SeesActiveEvent()
        {
            await _fixture.EnsureRunningAsync();

            var context = await _fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync($"{_fixture.WebBaseUrl}/?fakeuser=testuser", new() { Timeout = 60000 });

                var spinner = page.Locator(".loading-spinner.active");
                await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });

                await Assertions.Expect(page.Locator("text=Welkom bij")).ToBeVisibleAsync();

                var eventTiles = page.Locator(".tile.tile-event");

                // Wacht tot minimaal 1 tile zichtbaar is
                await eventTiles.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 60000 });

                int count = await eventTiles.CountAsync();
                Assert.IsTrue(count > 0, "Geen evenementen gevonden op de homepage");

                var firstEventTile = page.Locator(".tile.tile-event").First;
                await firstEventTile.ClickAsync();
                await Assertions.Expect(page).ToHaveURLAsync(new Regex(@".*/Event\?EventId=\d+"));

                var eventTitle = page.Locator("h1");
                await Assertions.Expect(eventTitle).ToHaveTextAsync("Tour de Test");

                var eventSlogan = page.Locator("#slogan");
                await Assertions.Expect(eventSlogan).ToHaveTextAsync("test test test");

                Console.WriteLine("Event klik werkt en navigatie naar detailpagina is gelukt!");
            }
            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
            }
        }

        [TestMethod]
        public async Task Logout_Works_Correctly()
        {
            await _fixture.EnsureRunningAsync();

            var context = await _fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Log in als fakeuser
            await page.GotoAsync($"{_fixture.WebBaseUrl}/?fakeuser=testuser", new() { Timeout = 60000 });

            // Wacht tot Profiel-link zichtbaar is
            var profielLink = page.GetByRole(AriaRole.Link, new() { Name = "Profiel" });
            await Assertions.Expect(profielLink).ToBeVisibleAsync(new() { Timeout = 10000 });

            // Klik op Profiel
            await profielLink.ClickAsync();

            // Klik op de logout button in de profielpagina/menu
            var logoutButton = page.GetByRole(AriaRole.Button, new() { Name = "Uitloggen" });
            await Assertions.Expect(logoutButton).ToBeVisibleAsync();
            await logoutButton.ClickAsync();

            await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Profiel" }))
                .ToHaveCountAsync(0);

            // Controleer dat we terug op de homepage of loginpagina komen
            await Assertions.Expect(page).ToHaveURLAsync(new Regex(@".*/Account/Login|.*/$"));

            // Optioneel: Profiel-link is niet meer zichtbaar
            var profielLinks = page.GetByRole(AriaRole.Link, new() { Name = "Profiel" });
            Assert.IsFalse(await profielLinks.IsVisibleAsync());

            await context.CloseAsync();
        }
    }
}
