using Microsoft.Playwright;

namespace CycleManager.Tests.E2E
{
    [TestClass]
    public class PoolTests : BaseE2ETest
    {
        [TestMethod]
        public async Task CreateEditCancelDeletePool_Works()
        {
            // Log in als gebruiker
            await LoginAsAsync("testuser");
            await WaitForAppReadyAsync();

            var profielLink = Page.GetByRole(AriaRole.Link, new() { Name = "Profiel" });
            await Assertions.Expect(profielLink).ToBeVisibleAsync(new() { Timeout = 15000 });
            await profielLink.ClickAsync();

            // Klik op details van het eerste toekomstige evenement
            var futureEvent = Page.Locator(".card:has-text('Toekomstige evenementen') li.list-group-item").First;
            var detailsButton = futureEvent.Locator("button:text-is('Details')");
            await detailsButton.ClickAsync();

            // Wacht tot '+ Pool aanmaken' zichtbaar is
            var addPoolButton = futureEvent.Locator("button:text-is('+ Pool aanmaken')");
            await Assertions.Expect(addPoolButton).ToBeVisibleAsync();

            // Maak een nieuwe pool
            await addPoolButton.ClickAsync();
            var poolNameInput = futureEvent.Locator("input[name='poolNaam']");
            await poolNameInput.FillAsync("Mijn Test Pool");
            await futureEvent.Locator("button:text-is('Voeg toe')").ClickAsync();

            // Klik op details van het eerste toekomstige evenement
            await detailsButton.ClickAsync();

            // Controleer dat de pool in de lijst verschijnt
            var createdPool = futureEvent.Locator(".accordion-item:has-text('Mijn Test Pool')");
            await Assertions.Expect(createdPool).ToBeVisibleAsync();

            // Test annuleren van een nieuwe pool
            await addPoolButton.ClickAsync();
            await futureEvent.Locator("button:text-is('Annuleren')").ClickAsync();
            var cancelledPool = futureEvent.Locator(".accordion-item:has-text('')"); // geen naam ingevuld
            Assert.AreEqual(2, await cancelledPool.CountAsync());

            // Klik op de aangemaakte pool en controleer opties
            await createdPool.Locator(".accordion-button").ClickAsync();

            //var editTeamLink = futureEvent.Locator("a", new() { HasText = "Bewerk je team!" });
            await Page.Locator(".accordion-item:has-text('Mijn Test Pool')")
                      .GetByRole(AriaRole.Link, new() { Name = "Bewerk je team!" })
                      .ClickAsync();

            await Assertions.Expect(Page.Locator("h2#pageTitle"))
            .ToHaveTextAsync("Renners-selectie (0 van 15)");
        }
    }
}
