using CycleManager.Services;
using CycleManager.Services.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using WebCycle.Services;


namespace CycleManager.Tests.E2E
{
    [TestClass]
    public class PoolTests
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
        public async Task PoolPage_Works_Correctly()
        {
            await _fixture.EnsureRunningAsync();

            var browserContext = await _fixture.Browser.NewContextAsync();
            var page = await browserContext.NewPageAsync();

            try
            {
                // === STEP 1: Open eventpagina ===
                await page.GotoAsync($"{_fixture.WebBaseUrl}", new() { Timeout = 60000 });

                // === STEP 2: Open eerste event voor tussenstand ===
                var firstEventTile = page.Locator(".tile.tile-event").First;

                // Wacht tot de tile zichtbaar is
                await firstEventTile.WaitForAsync(new() { State = WaitForSelectorState.Visible });

                // Klik op de tile om naar de detailpagina te gaan
                await firstEventTile.ClickAsync();

                // === STEP 3: Controleer event header ===
                var eventTitle = await page.Locator("#eventName").InnerTextAsync();
                var startDate = await page.Locator("#startDate").InnerTextAsync();
                var endDate = await page.Locator("#endDate").InnerTextAsync();
                Console.WriteLine($"Event: {eventTitle}, Start: {startDate}, Eind: {endDate}");

                // === STEP 4: Controleer stages ===
                var stages = page.Locator("#steps-container .step");
                var stageCount = await stages.CountAsync();
                Console.WriteLine($"Aantal stages: {stageCount}");

                for (int i = 0; i < stageCount; i++)
                {
                    var stage = stages.Nth(i);
                    var completed = await stage.Locator("a").EvaluateAsync<bool>("el => el.parentElement.classList.contains('completed')");
                    Console.WriteLine($"Stage {i + 1}: {(completed ? "verreden" : "nog te verreden")}");
                }

                // === STEP 5: Controleer ranglijst deelnemers ===
                var deelnemers = page.Locator("#deelnemer-list > li");
                var deelnemerCount = await deelnemers.CountAsync();
                Console.WriteLine($"Aantal deelnemers: {deelnemerCount}");

                for (int i = 0; i < deelnemerCount; i++)
                {
                    var deelnemer = deelnemers.Nth(i);
                    var rank = await deelnemer.Locator("div.row > div").Nth(0).InnerTextAsync();
                    var poolName = await deelnemer.Locator("div.row > div").Nth(1).InnerTextAsync();
                    var participantName = await deelnemer.Locator("div.row > div").Nth(2).InnerTextAsync();
                    var points = await deelnemer.Locator("div.row > div").Nth(3).InnerTextAsync();

                    Console.WriteLine($"Deelnemer {rank}: {poolName} ({participantName}) - Punten: {points}");

                    // Optioneel: klap de deelnemer open om individuele renners te controleren
                    await deelnemer.Locator("div.row").ClickAsync();
                    var renners = deelnemer.Locator(".details-content p");
                    var rennersCount = await renners.CountAsync();
                    for (int j = 0; j < rennersCount; j++)
                    {
                        var rennerInfo = await renners.Nth(j).InnerTextAsync();
                        Console.WriteLine($"   Renner: {rennerInfo}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Test gefaald: " + ex.Message);

                var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "pool_error.png");
                await page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                Console.WriteLine($"Screenshot opgeslagen: {screenshotPath}");

                var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "pool_error.html");
                await File.WriteAllTextAsync(htmlPath, await page.ContentAsync());
                Console.WriteLine($"HTML dump opgeslagen: {htmlPath}");

                throw;
            }
            finally
            {
                await page.CloseAsync();
                await browserContext.CloseAsync();
            }
        }

        [TestMethod]
        public async Task PoolPage_Scores_Are_Correct_From_Seed()
        {
            await _fixture.EnsureRunningAsync();

            var ctx = await _fixture.Browser.NewContextAsync();
            var page = await ctx.NewPageAsync();

            await page.GotoAsync(_fixture.WebBaseUrl);
            var firstEventTile = page.Locator(".tile.tile-event").First;
            await firstEventTile.WaitForAsync();
            await firstEventTile.ClickAsync();

            var deelnemers = page.Locator("#deelnemer-list > li");
            Assert.AreEqual(1, await deelnemers.CountAsync());

            var pool = deelnemers.First;

            var poolNameUi = (await pool.Locator("div.row > div").Nth(1).InnerTextAsync()).Trim();
            var parNameUi = (await pool.Locator("div.row > div").Nth(2).InnerTextAsync()).Trim();
            var totalScoreText = (await pool.Locator("div.fw-bold.fs-4").InnerTextAsync()).Trim();

            var match = System.Text.RegularExpressions.Regex.Match(totalScoreText, @"\d+");
            int pointsUi = match.Success ? int.Parse(match.Value) : 0;

            Assert.IsTrue(
                string.Equals(poolNameUi.Trim(), "E2E Pool", StringComparison.InvariantCultureIgnoreCase),
                $"Poolnaam mismatch: verwacht 'E2E Pool' maar UI gaf '{poolNameUi}'"
            );

            Assert.IsTrue(
                string.Equals(parNameUi.Trim(), "E2E Tester", StringComparison.InvariantCultureIgnoreCase),
                $"Deelnemer mismatch: verwacht 'E2E Tester' maar UI gaf '{parNameUi}'"
            );


            // Open renners
            await pool.Locator("div.row").ClickAsync();

            var renners = pool.Locator(".details-content .renner-item");

            // Wacht tot er 8 renners zichtbaar zijn
            await Assertions.Expect(renners).ToHaveCountAsync(8, new() { Timeout = 5000 });

            // --- Vul uiRenners (ongewijzigd, alleen we halen globalIndex robuust) ---
            var uiRenners = new List<(string Name, int Points, int GlobalIndex)>();
            for (int i = 0; i < await renners.CountAsync(); i++)
            {
                var item = renners.Nth(i);
                string name = (await item.Locator("div.fw-bold").First.InnerTextAsync()).Trim();
                string rawPoints = (await item.Locator("span.fw-bold.fs-5").InnerTextAsync()).Trim();
                var mPoints = System.Text.RegularExpressions.Regex.Match(rawPoints, @"-?\d+");
                int points = mPoints.Success ? int.Parse(mPoints.Value) : 0;

                // ROBUUST parse van teamId en riderNum uit "R. ider_18_6"
                var mName = System.Text.RegularExpressions.Regex.Match(name, @"(\d+)\D+(\d+)$");
                if (!mName.Success)
                {
                    Console.WriteLine($"DEBUG: kon team/rider niet parsen uit naam: '{name}'");
                    throw new Exception($"Kon team/rider niet parsen uit naam: '{name}'");
                }
                int teamId = int.Parse(mName.Groups[1].Value);
                int riderNum = int.Parse(mName.Groups[2].Value);

                // globalIndex 0-based exactly zoals in seed: (teamId-1)*10 + (riderNum-1)
                int globalIndex = (teamId - 1) * 10 + (riderNum - 1);

                Console.WriteLine($"DEBUG: UI renner '{name}' -> team {teamId}, rider {riderNum}, globalIndex {globalIndex}, points={points}");

                uiRenners.Add((name, points, globalIndex));
            }

            // --- Bereken expected, exact dezelfde logica als seed ---
            // parameters - pas aan als jouw seed anders is
            int teams = 20;
            int ridersPerTeam = 10;
            int cieCount = teams * ridersPerTeam;   // 200
            int stageCount = 21;

            int[] top20Points = { 50, 40, 35, 30, 25, 20, 18, 16, 14, 12, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

            // BEREKEN pickIndices op exact dezelfde manier als seed:
            // seed deed: for (i=0;i<8;i++){ int idx = (i * (cieCount / 8)) % cieCount; picks.Add(cieList[idx]); }
            int picksToTake = 8;
            var pickIndices = new List<int>();
            for (int i = 0; i < picksToTake; i++)
            {
                int idx = (i * (cieCount / picksToTake)) % cieCount;
                pickIndices.Add(idx);
            }
            Console.WriteLine("DEBUG: pickIndices = " + string.Join(", ", pickIndices));

            // expectedPerRider keyed by 0-based globalIndex
            var expectedPerRider = new Dictionary<int, int>();
            for (int idx = 0; idx < cieCount; idx++) expectedPerRider[idx] = 0;

            int expectedTotal = 0;
            for (int sIndex = 0; sIndex < stageCount; sIndex++)
            {
                for (int pos = 1; pos <= 20; pos++)
                {
                    int competitorIndex = (sIndex * 20 + (pos - 1)) % cieCount;
                    if (pickIndices.Contains(competitorIndex))
                    {
                        int pts = top20Points[pos - 1];
                        expectedPerRider[competitorIndex] += pts;
                        expectedTotal += pts;
                    }
                }
            }

            // --- Debug: toon expected value voor alle UI-renners (handig) ---
            foreach (var (name, pointsUiR, globalIndex) in uiRenners)
            {
                expectedPerRider.TryGetValue(globalIndex, out var expectedForThis);
                Console.WriteLine($"DEBUG: UI renner '{name}' globalIndex={globalIndex}, expected={expectedForThis}, ui={pointsUiR}, isPick={pickIndices.Contains(globalIndex)}");
            }

            // --- Asserts: controleer alleen renners die in pool (dus uiRenners bevat picks) ---
            // if UI shows renners that are not picks, those should have expected 0
            foreach (var (name, pointsUiR, globalIndex) in uiRenners)
            {
                if (!expectedPerRider.ContainsKey(globalIndex))
                {
                    Console.WriteLine($"DEBUG: expectedPerRider does not contain globalIndex {globalIndex} for {name}");
                    Assert.Fail($"expectedPerRider missing for globalIndex {globalIndex} for {name}");
                }

                int expected = expectedPerRider[globalIndex];
                // extra debug on mismatch
                if (expected != pointsUiR)
                {
                    Console.WriteLine($"MISMATCH: renner {name} (global {globalIndex}) expected={expected} but UI={pointsUiR}");
                    Console.WriteLine($"DEBUG: pickIndices = {string.Join(",", pickIndices)}");
                }

                Assert.AreEqual(expected, pointsUiR, $"Rennerscore fout voor {name}: UI={pointsUiR} vs expected={expected}");
            }

            // Controleer totaalscore
            Assert.AreEqual(expectedTotal, pointsUi, $"Totale score fout: UI={pointsUi} vs expected={expectedTotal}");
        }

        [TestMethod]
        public async Task PoolPage_CustomPointsConfiguration_Works()
        {
            using var scope = _fixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // --- 1. Haal bestaand event en deelnemer op ---
            var ev = await db.Events
                .Include(e => e.Configuration)
                    .ThenInclude(c => c.ConfigurationItems)
                .Include(e => e.Stages)
                .Include(e => e.GameCompetitorEvents)
                    .ThenInclude(gce => gce.Renners)
                .FirstAsync(e => e.EventName == "E2E Test Event");

            var deelnemer = ev.GameCompetitorEvents.First();

            // --- 2. Maak nieuwe custom configuratie ---
            int[] customPoints = { 30, 27, 24, 21, 19, 17, 15, 13, 11, 9, 8, 7, 5, 3, 1 }; // 15 posities
            var config = new Configuration { ConfigurationType = "Custom Test Config" };
            db.Configurations.Add(config);
            await db.SaveChangesAsync();

            for (int i = 0; i < customPoints.Length; i++)
            {
                db.ConfigurationItems.Add(new ConfigurationItem
                {
                    ConfigurationId = config.Id,
                    Position = i + 1,
                    Score = customPoints[i]
                });
            }
            await db.SaveChangesAsync();

            // --- 3. Koppel configuratie aan event en verwijder oude scores ---
            ev.ConfigurationId = config.Id;

            var pickIds = ev.GameCompetitorEvents.SelectMany(gce => gce.Renners).Select(p => p.Id).ToList();
            db.DeelnemerPickScores.RemoveRange(db.DeelnemerPickScores.Where(dps => pickIds.Contains(dps.GameCompetitorEventPickId)));
            db.DeelnemerScores.RemoveRange(db.DeelnemerScores.Where(ds => ds.Stage.EventId == ev.EventId));

            await db.SaveChangesAsync();

            // --- 4. Recalculate resultaten via je ResultService ---
            var resultService = scope.ServiceProvider.GetRequiredService<IResultService>();
            await resultService.RecalculateEventScoresAsync(ev.EventId);

            // --- 5. Controleer individuele pick scores ---
            var updatedPickScores = db.DeelnemerPickScores
                .Include(dps => dps.Pick)
                .Where(dps => pickIds.Contains(dps.GameCompetitorEventPickId))
                .ToList();

            foreach (var pickScore in updatedPickScores)
            {
                // Haal de resultaten van de renner in de stage
                var resultsForPick = db.Results
                    .Where(r => r.CompetitorInEventId == pickScore.Pick.CompetitorsInEventId)
                    .ToList();

                int expectedPickTotal = 0;
                int expectedLastStageScore = 0;
                int? expectedLastStageId = null;

                foreach (var r in resultsForPick.OrderBy(r => r.StageId))
                {
                    var ci = db.ConfigurationItems.FirstOrDefault(c => c.Id == r.ConfigurationItemId);
                    if (ci != null)
                    {
                        expectedPickTotal += ci.Score;
                        expectedLastStageScore = ci.Score;
                        expectedLastStageId = r.StageId;
                    }
                }

                // 1) Score = totaal over alle stages
                Assert.AreEqual(expectedPickTotal, pickScore.Score,
                    $"Pick total fout voor {pickScore.GameCompetitorEventPickId}");

                // 2) StageId = laatste stageId (nullable vergelijking)
                Assert.AreEqual(expectedLastStageId, pickScore.StageId,
                    $"Pick laatste stageId fout voor {pickScore.GameCompetitorEventPickId}");


                if (expectedLastStageId.HasValue)
                {
                    // vind de result voor die renner in die stage en lees de CI -> score
                    var lastResult = db.Results
                        .FirstOrDefault(r => r.CompetitorInEventId == pickScore.Pick.CompetitorsInEventId
                                          && r.StageId == expectedLastStageId.Value);

                    var lastStageCi = lastResult == null ? null
                        : db.ConfigurationItems.FirstOrDefault(ci => ci.Id == lastResult.ConfigurationItemId);

                    var lastStageScoreFromResults = lastStageCi?.Score ?? 0;

                    Assert.AreEqual(expectedLastStageScore, lastStageScoreFromResults,
                        $"Pick laatste stage-score (uit Results) fout voor {pickScore.GameCompetitorEventPickId}");
                }
                else
                {
                    // geen laatste stage, verwacht null
                    Assert.IsNull(expectedLastStageId, $"Expected no last stage but found pickStage {pickScore.StageId}");
                }
            }

            // --- 6. Controleer DeelnemerScore ---
            var deelnemerScore = db.DeelnemerScores.First(ds => ds.GameCompetitorEventId == deelnemer.Id);

            int totalExpected = updatedPickScores.Sum(ps => ps.Score);

            int lastStageIdExpected = updatedPickScores.Max(ps => ps.StageId) ?? 0;
            var picksForDeelnemer = updatedPickScores;
            int lastStageScoreExpected = picksForDeelnemer.Sum(ps =>
            {
                var r = db.Results.FirstOrDefault(r =>
                    r.CompetitorInEventId == ps.Pick.CompetitorsInEventId &&
                    r.StageId == lastStageIdExpected);

                if (r == null || r.ConfigurationItemId == null)
                    return 0;

                var ci = db.ConfigurationItems.First(ci => ci.Id == r.ConfigurationItemId);
                return ci.Score;
            });

            Assert.AreEqual(totalExpected, deelnemerScore.TotalScore, "Totale score deelnemer fout");
            Assert.AreEqual(lastStageScoreExpected, deelnemerScore.LaatsteScore, "Laatste score deelnemer fout");
            Assert.AreEqual(lastStageIdExpected, deelnemerScore.StageId, "Laatste stageId deelnemer fout");
        }

        [TestMethod]
        public async Task PoolPage_Works_ForTop20AndCustom15()
        {
            await _fixture.EnsureRunningAsync();

            var browserContext = await _fixture.Browser.NewContextAsync();
            var page = await browserContext.NewPageAsync();

            try
            {
                // 1️⃣ Loop over beide events/configs
                var eventConfigs = new[]
                {
                new { EventName = "E2E Top20 Event", Points = SeedData.Top20Points, Picks = 8 },
                new { EventName = "E2E Custom15 Event", Points = SeedData.Custom15Points, Picks = 8 }
            };

                using var scope = _fixture.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                foreach (var ec in eventConfigs)
                {
                    // Haal event + pool
                    var ev = await db.Events
                        .Include(e => e.GameCompetitorEvents)
                        .ThenInclude(gce => gce.Renners)
                        .FirstAsync(e => e.EventName == ec.EventName);

                    var pool = ev.GameCompetitorEvents.First();

                    // --- Ga naar eventpagina ---
                    await page.GotoAsync(_fixture.WebBaseUrl, new() { Timeout = 60000 });
                    var firstEventTile = page.Locator($".tile.tile-event:has-text('{ev.EventName}')").First;
                    await firstEventTile.WaitForAsync(new() { State = WaitForSelectorState.Visible });
                    await firstEventTile.ClickAsync();

                    // --- Wacht op deelnemerlijst ---
                    await page.WaitForSelectorAsync("#deelnemer-list", new() { Timeout = 15000 });
                    var poolItem = page.Locator("#deelnemer-list > li").First;
                    await poolItem.WaitForAsync(new() { State = WaitForSelectorState.Visible });

                    // Open pool details
                    await poolItem.Locator("div.row").ClickAsync();
                    var renners = poolItem.Locator(".renner-item");
                    int rennerCount = await renners.CountAsync();

                    // --- UI uitlezen ---
                    var uiRenners = new List<(string Name, int Points, int GlobalIndex)>();
                    for (int i = 0; i < rennerCount; i++)
                    {
                        var item = renners.Nth(i);
                        string name = (await item.Locator("div.fw-bold").First.InnerTextAsync()).Trim();
                        string rawPoints = (await item.Locator("span.fw-bold.fs-5").InnerTextAsync()).Trim();
                        int points = int.TryParse(rawPoints, out var p) ? p : 0;

                        var mName = System.Text.RegularExpressions.Regex.Match(name, @"(\d+)\D+(\d+)$");
                        if (!mName.Success)
                            throw new Exception($"Kon team/rider niet parsen uit naam: '{name}'");

                        int teamId = int.Parse(mName.Groups[1].Value);
                        int riderNum = int.Parse(mName.Groups[2].Value);
                        int globalIndex = (teamId - 1) * 10 + (riderNum - 1);

                        uiRenners.Add((name, points, globalIndex));
                    }

                    // --- Bereken expected scores uit seed-logica ---
                    int teams = 20;
                    int ridersPerTeam = 10;
                    int cieCount = teams * ridersPerTeam;
                    int stageCount = 21;

                    // Bereken pickIndices exact zoals seed
                    var pickIndices = new List<int>();
                    for (int i = 0; i < ec.Picks; i++)
                        pickIndices.Add((i * (cieCount / ec.Picks)) % cieCount);

                    var expectedPerRider = new Dictionary<int, int>();
                    for (int idx = 0; idx < cieCount; idx++) expectedPerRider[idx] = 0;

                    int expectedTotal = 0;
                    for (int sIndex = 0; sIndex < stageCount; sIndex++)
                    {
                        for (int pos = 1; pos <= ec.Points.Length; pos++)
                        {
                            int competitorIndex = (sIndex * ec.Points.Length + (pos - 1)) % cieCount;
                            if (pickIndices.Contains(competitorIndex))
                            {
                                expectedPerRider[competitorIndex] += ec.Points[pos - 1];
                                expectedTotal += ec.Points[pos - 1];
                            }
                        }
                    }

                    // --- Vergelijk UI vs expected ---
                    foreach (var (name, pointsUiR, globalIndex) in uiRenners)
                    {
                        int expected = expectedPerRider[globalIndex];
                        Assert.AreEqual(expected, pointsUiR, $"[{ec.EventName}] Rennerscore fout voor {name}: UI={pointsUiR} vs expected={expected}");
                    }

                    // --- Totale score vergelijken ---
                    var totalScoreText = await poolItem.Locator("div.fw-bold.fs-4").InnerTextAsync();
                    var match = System.Text.RegularExpressions.Regex.Match(totalScoreText, @"\d+");
                    int pointsUi = match.Success ? int.Parse(match.Value) : 0;
                    Assert.AreEqual(expectedTotal, pointsUi, $"[{ec.EventName}] Totale score mismatch: UI={pointsUi}, Expected={expectedTotal}");
                }
            }
            finally
            {
                await page.CloseAsync();
                await browserContext.CloseAsync();
            }
        }
    }
}