using Microsoft.Playwright;

namespace CycleManager.Tests.E2E
{
    [TestClass]
    public class PoolTests : BaseE2ETest
    {
        //[TestMethod]
        //public async Task CreateEditCancelDeletePool_Works()
        //{
        //    // Log in als gebruiker
        //    await LoginAsAsync("testuser");
        //    await WaitForAppReadyAsync();

        //    var profielLink = Page.GetByRole(AriaRole.Link, new() { Name = "Profiel" });
        //    await Assertions.Expect(profielLink).ToBeVisibleAsync(new() { Timeout = 15000 });
        //    await profielLink.ClickAsync();

        //    // Klik op details van het eerste toekomstige evenement
        //    var futureEvent = Page.Locator(".card:has-text('Toekomstige evenementen') li.list-group-item").First;
        //    var detailsButton = futureEvent.Locator("button:text-is('Details')");
        //    await detailsButton.ClickAsync();

        //    // Wacht tot '+ Pool aanmaken' zichtbaar is
        //    var addPoolButton = futureEvent.Locator("button:text-is('+ Pool aanmaken')");
        //    await Assertions.Expect(addPoolButton).ToBeVisibleAsync();

        //    // Maak een nieuwe pool
        //    await addPoolButton.ClickAsync();
        //    var poolNameInput = futureEvent.Locator("input[name='poolNaam']");
        //    await poolNameInput.FillAsync("Mijn Test Pool");
        //    await futureEvent.Locator("button:text-is('Voeg toe')").ClickAsync();

        //    // Klik op details van het eerste toekomstige evenement
        //    await detailsButton.ClickAsync();

        //    // Controleer dat de pool in de lijst verschijnt
        //    var createdPool = futureEvent.Locator(".accordion-item:has-text('Mijn Test Pool')");
        //    await Assertions.Expect(createdPool).ToBeVisibleAsync();

        //    // Test annuleren van een nieuwe pool
        //    await addPoolButton.ClickAsync();
        //    await futureEvent.Locator("button:text-is('Annuleren')").ClickAsync();
        //    var cancelledPool = futureEvent.Locator(".accordion-item:has-text('')"); // geen naam ingevuld
        //    Assert.AreEqual(2, await cancelledPool.CountAsync());

        //    // Klik op de aangemaakte pool en controleer opties
        //    await createdPool.Locator(".accordion-button").ClickAsync();

        //    await Page.Locator(".accordion-item:has-text('Mijn Test Pool')")
        //              .GetByRole(AriaRole.Link, new() { Name = "Bewerk je team!" })
        //              .ClickAsync();

        //    await Assertions.Expect(Page.Locator("h2#pageTitle"))
        //        .ToHaveTextAsync("Renners-selectie (0 van 15)");
        //}
        [TestMethod]
        public async Task CreateEditCancelDeletePool_Works()
        {
            // === STEP 1: Log in en open profiel ===
            await LoginAsAsync("testuser");
            await WaitForAppReadyAsync();

            var profielLink = Page.GetByRole(AriaRole.Link, new() { Name = "Profiel" });
            await Assertions.Expect(profielLink).ToBeVisibleAsync(new() { Timeout = 15000 });
            await profielLink.ClickAsync();

            // === STEP 2: Open toekomstig evenement ===
            var futureEvent = Page.Locator(".card", new() { HasText = "Toekomstige evenementen" });
            var detailsButton = futureEvent.GetByRole(AriaRole.Button, new() { Name = "Details" });
            await detailsButton.ClickAsync();

            await Page.WaitForSelectorAsync("button:text-is('+ Pool aanmaken')");

            // === STEP 3: Maak nieuwe pool aan ===
            var addPoolButton = futureEvent.GetByRole(AriaRole.Button, new() { Name = "+ Pool aanmaken" });
            await Assertions.Expect(addPoolButton).ToBeVisibleAsync();
            await addPoolButton.ClickAsync();

            var poolNameInput = futureEvent.Locator("input[name='poolNaam']");
            await poolNameInput.FillAsync("Mijn Test Pool");
            await futureEvent.GetByRole(AriaRole.Button, new() { Name = "Voeg toe" }).ClickAsync();

            // Wacht tot de nieuwe pool verschijnt
            var createdPool = futureEvent.Locator(".accordion-item:has-text('Mijn Test Pool')");
            await Assertions.Expect(createdPool).ToBeVisibleAsync();

            // === STEP 4: Test annuleren van een nieuwe pool ===
            await addPoolButton.ClickAsync();
            await futureEvent.GetByRole(AriaRole.Button, new() { Name = "Annuleren" }).ClickAsync();

            // Controleer dat er nu 2 accordion-items aanwezig zijn (1 echte + 1 geannuleerde)
            var allPools = futureEvent.Locator(".accordion-item");
            await Assertions.Expect(allPools).ToHaveCountAsync(2);

            // === STEP 5: Bewerk de aangemaakte pool ===
            await createdPool.Locator(".accordion-button").ClickAsync();

            var editLink = Page
                .Locator(".accordion-item:has-text('Mijn Test Pool')")
                .GetByRole(AriaRole.Link, new() { Name = "Bewerk je team!" });

            await Assertions.Expect(editLink).ToBeVisibleAsync();
            await editLink.ClickAsync();

            // === STEP 6: Controleer de selectiepagina ===
            var header = Page.Locator("h2#pageTitle");
            await Assertions.Expect(header)
                .ToHaveTextAsync(new Regex(@"Renners-selectie\s*\(\s*0\s*van\s*15\s*\)"));

        }
    }
}
