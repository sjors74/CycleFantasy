using Microsoft.Playwright;

namespace CycleManager.Tests.E2E
{
    [TestClass]
    public class HomeTests : BaseE2ETest
    {
        [TestMethod]
        public async Task Homepage_Has_MenuAndWelcomMessage()
        {
            await Page.GotoAsync(WebBaseUrl);
            await WaitForAppReadyAsync();

            var welcomeText = Page.Locator("text=Welkom bij Tourmanager");
            Assert.IsTrue(await welcomeText.IsVisibleAsync(), "De welkomstekst is niet zichtbaar.");
        }

        [TestMethod]
        public async Task Homepage_Navigation_Works_Correctly()
        {
            await Page.GotoAsync(WebBaseUrl);
            await WaitForAppReadyAsync();

            await Assertions.Expect(Page.Locator("text=Welkom bij Tourmanager")).ToBeVisibleAsync();

            await Page.GetByRole(AriaRole.Link, new() { Name = "Inloggen" }).ClickAsync();
            await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/Account/Login"));

            await Page.GetByRole(AriaRole.Link, new() { Name = "Registreren" }).ClickAsync();
            await Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/Account/Register"));

            await Page.GetByRole(AriaRole.Link, new() { Name = "Home" }).ClickAsync();
            await Assertions.Expect(Page.Locator("text=Welkom bij Tourmanager")).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task Login_WithValidCredentials_Works()
        {
            await LoginAsAsync("testuser");

            var profielLink = Page.GetByRole(AriaRole.Link, new() { Name = "Profiel" });
            await Assertions.Expect(profielLink).ToBeVisibleAsync(new() { Timeout = 15000 });
        }

        [TestMethod]
        public async Task Homepage_Shows_LoginLink_WhenUserNotSignedIn()
        {
            await Page.GotoAsync(WebBaseUrl);
            await WaitForAppReadyAsync();

            await Assertions.Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Inloggen" }))
                .ToBeVisibleAsync();

            await Assertions.Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Profiel" }))
                .Not.ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task LoggedInUser_SeesActiveEvent()
        {
            await LoginAsAsync("testuser");

            await Assertions.Expect(Page.Locator("text=Welkom bij")).ToBeVisibleAsync();

            var eventTiles = Page.Locator(".tile.tile-event");
            await eventTiles.First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var firstEventTile = eventTiles.First;
            await firstEventTile.ClickAsync();

            await Assertions.Expect(Page).ToHaveURLAsync(new Regex(@".*/Event\?EventId=\d+"));
            await Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Tour de Test");
            await Assertions.Expect(Page.Locator("#slogan")).ToHaveTextAsync("test test test");
        }

        [TestMethod]
        public async Task Logout_Works_Correctly()
        {
            await LoginAsAsync("testuser");

            var profielLink = Page.GetByRole(AriaRole.Link, new() { Name = "Profiel" });
            await profielLink.ClickAsync();

            var logoutButton = Page.GetByRole(AriaRole.Button, new() { Name = "Uitloggen" });
            await Assertions.Expect(logoutButton).ToBeVisibleAsync();
            await logoutButton.ClickAsync();

            await Assertions.Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Profiel" }))
                .ToHaveCountAsync(0);

            await Assertions.Expect(Page).ToHaveURLAsync(new Regex(@".*/Account/Login|.*/$"));
        }
    }
}