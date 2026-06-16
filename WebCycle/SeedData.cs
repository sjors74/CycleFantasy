using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Models;

namespace WebCycle.Services
{
    public static class SeedData
    {
        public static readonly int[] Top20Points = { 50, 40, 35, 30, 25, 20, 18, 16, 14, 12, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        public static readonly int[] Custom15Points = { 30, 27, 24, 21, 19, 17, 15, 13, 11, 9, 8, 7, 5, 3, 1 };

        public static async Task EnsureSeedAsync(ApplicationDbContext context)
        {
            if (context.Events.Any())
            {
                Console.WriteLine("SeedData: Events already exist");
                return;
            }

            Console.WriteLine("Seeding Events...");

            // ----------------------
            // 1. Land
            // ----------------------
            var country = new Country { CountryNameLong = "Italy", CountryNameShort = "it" };
            context.Countries.Add(country);
            await context.SaveChangesAsync();

            // ----------------------
            // 2. Configuraties
            // ----------------------
            var top20Config = new Configuration { ConfigurationType = "E2E Top20" };
            var custom15Config = new Configuration { ConfigurationType = "E2E Custom15" };
            context.Configurations.AddRange(top20Config, custom15Config);
            await context.SaveChangesAsync();

            for (int pos = 1; pos <= 20; pos++)
            {
                context.ConfigurationItems.Add(new ConfigurationItem
                {
                    Position = pos,
                    Score = Top20Points[pos - 1],
                    ConfigurationId = top20Config.Id
                });
            }

            for (int pos = 1; pos <= Custom15Points.Length; pos++)
            {
                context.ConfigurationItems.Add(new ConfigurationItem
                {
                    Position = pos,
                    Score = Custom15Points[pos - 1],
                    ConfigurationId = custom15Config.Id
                });
            }
            await context.SaveChangesAsync();

            // ----------------------
            // 3. Events
            // ----------------------
            var evTop20 = new Event
            {
                EventName = "E2E Top20 Event",
                CountryCode = country.CountryNameShort,
                ColorName = "#00F0F0",
                EventYear = DateTime.UtcNow.Year,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(21),
                IsActive = true,
                ConfigurationId = top20Config.Id
            };

            var evCustom15 = new Event
            {
                EventName = "E2E Custom15 Event",
                CountryCode = country.CountryNameShort,
                ColorName = "#F0A000",
                EventYear = DateTime.UtcNow.Year,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(21),
                IsActive = true,
                ConfigurationId = custom15Config.Id
            };
            context.Events.AddRange(evTop20, evCustom15);
            await context.SaveChangesAsync();

            // ----------------------
            // 4. Teams
            // ----------------------
            var teams = new List<Team>();
            for (int t = 1; t <= 20; t++)
            {
                var team = new Team { CurrentTeamName = $"Team {t}", CountryId = country.CountryId };
                teams.Add(team);
                context.Teams.Add(team);
            }
            await context.SaveChangesAsync();

            // ----------------------
            // 5. TeamYears
            // ----------------------
            var teamYears = new List<CycleManager.Domain.Models.TeamYear>();
            foreach (var team in teams)
            {
                var ty = new CycleManager.Domain.Models.TeamYear
                {
                    TeamId = team.TeamId,
                    Year = 2025,
                    Name = $"{team.CurrentTeamName}-2025"
                };
                teamYears.Add(ty);
                context.TeamYear.Add(ty);
            }
            await context.SaveChangesAsync();

            // ----------------------
            // 6. Competitors + CompetitorInTeams
            // ----------------------
            var competitorInTeams = new List<CompetitorInTeam>();
            for (int i = 0; i < teamYears.Count; i++)
            {
                var ty = teamYears[i];
                for (int r = 1; r <= 10; r++)
                {
                    var competitor = new Competitor
                    {
                        FirstName = "R.",
                        LastName = $"ider_{ty.TeamId}_{r}",
                        CountryId = country.CountryId
                    };
                    context.Competitors.Add(competitor);
                    await context.SaveChangesAsync();

                    var cit = new CompetitorInTeam
                    {
                        TeamId = ty.TeamId,
                        TeamYearId = ty.TeamYearId,
                        CompetitorId = competitor.CompetitorId,
                        Year = 2025
                    };
                    context.CompetitorInTeams.Add(cit);
                    competitorInTeams.Add(cit);
                }
            }
            await context.SaveChangesAsync();

            // ----------------------
            // 7. CompetitorsInEvent (apart per event!)
            // ----------------------
            var cieListTop20 = new List<CompetitorsInEvent>();
            var cieListCustom15 = new List<CompetitorsInEvent>();

            foreach (var cit in competitorInTeams)
            {
                var cieTop20 = new CompetitorsInEvent { EventId = evTop20.EventId, CompetitorInTeamId = cit.Id };
                var cieCustom15 = new CompetitorsInEvent { EventId = evCustom15.EventId, CompetitorInTeamId = cit.Id };
                context.CompetitorsInEvent.AddRange(cieTop20, cieCustom15);
                cieListTop20.Add(cieTop20);
                cieListCustom15.Add(cieCustom15);
            }
            await context.SaveChangesAsync();

            // ----------------------
            // 8. EventTeam linking
            // ----------------------
            foreach (var team in teams)
            {
                context.EventTeam.AddRange(
                    new EventTeam { EventId = evTop20.EventId, TeamId = team.TeamId },
                    new EventTeam { EventId = evCustom15.EventId, TeamId = team.TeamId }
                );
            }
            await context.SaveChangesAsync();

            // ----------------------
            // 9. Stages
            // ----------------------
            var stagesTop20 = new List<Stage>();
            var stagesCustom15 = new List<Stage>();

            for (int s = 1; s <= 21; s++)
            {
                var stTop20 = new Stage
                {
                    EventId = evTop20.EventId,
                    StageDate = DateTime.UtcNow.Date.AddDays(s - 1),
                    StageName = $"{s}",
                    StageOrder = s,
                    StartLocation = $"Start {s}",
                    FinishLocation = $"Finish {s}"
                };
                var stCustom15 = new Stage
                {
                    EventId = evCustom15.EventId,
                    StageDate = DateTime.UtcNow.Date.AddDays(s - 1),
                    StageName = $"{s}",
                    StageOrder = s,
                    StartLocation = $"Start {s}",
                    FinishLocation = $"Finish {s}"
                };
                context.Stages.AddRange(stTop20, stCustom15);
                stagesTop20.Add(stTop20);
                stagesCustom15.Add(stCustom15);
            }
            await context.SaveChangesAsync();

            // ----------------------
            // 10. Results per event
            // ----------------------
            void AddResults(List<CompetitorsInEvent> cieList, List<Stage> stages, Configuration config, int pointsLength)
            {
                int cieCount = cieList.Count;
                foreach (var stage in stages)
                {
                    for (int pos = 1; pos <= pointsLength; pos++)
                    {
                        int competitorIndex = (stage.StageOrder - 1) * pointsLength + (pos - 1) % cieCount;
                        var cie = cieList[competitorIndex];
                        var ci = context.ConfigurationItems.First(c => c.ConfigurationId == config.Id && c.Position == pos);
                        context.Results.Add(new Result
                        {
                            StageId = stage.Id,
                            CompetitorInEventId = cie.Id,
                            ConfigurationItemId = ci.Id
                        });
                    }
                }
            }

            AddResults(cieListTop20, stagesTop20, top20Config, Top20Points.Length);
            AddResults(cieListCustom15, stagesCustom15, custom15Config, Custom15Points.Length);
            await context.SaveChangesAsync();

            // ----------------------
            // 11. Test Users + GameCompetitorEvents (pools)
            // ----------------------
            var userTop20 = new ApplicationUser { Id = "e2e_user_top20", FirstName = "E2E", LastName = "TesterTop20", Email = "e2e_top20@test.local" };
            var userCustom15 = new ApplicationUser { Id = "e2e_user_custom15", FirstName = "E2E", LastName = "TesterCustom15", Email = "e2e_custom15@test.local" };
            context.Users.AddRange(userTop20, userCustom15);
            await context.SaveChangesAsync();

            var poolTop20 = new GameCompetitorEvent { EventId = evTop20.EventId, UserId = userTop20.Id, TeamName = "E2E Pool Top20" };
            var poolCustom15 = new GameCompetitorEvent { EventId = evCustom15.EventId, UserId = userCustom15.Id, TeamName = "E2E Pool Custom15" };
            context.GameCompetitorsEvent.AddRange(poolTop20, poolCustom15);
            await context.SaveChangesAsync();

            // ----------------------
            // 12. Picks per pool
            // ----------------------
            void AddPicks(GameCompetitorEvent pool, List<CompetitorsInEvent> cieList)
            {
                int cieCount = cieList.Count;
                for (int i = 0; i < 8; i++)
                {
                    int idx = (i * (cieCount / 8)) % cieCount;
                    context.GameCompetitorEventPicks.Add(new GameCompetitorEventPick
                    {
                        GameCompetitorEventId = pool.Id,
                        CompetitorsInEventId = cieList[idx].Id
                    });
                }
            }

            AddPicks(poolTop20, cieListTop20);
            AddPicks(poolCustom15, cieListCustom15);
            await context.SaveChangesAsync();

            // ----------------------
            // 13. Bereken DeelnemerScores + DeelnemerPickScores per event
            // ----------------------
            async Task AddScores(GameCompetitorEvent pool, List<CompetitorsInEvent> cieList, List<Stage> stages, Configuration config, int[] points)
            {
                var pickList = context.GameCompetitorEventPicks.Where(p => p.GameCompetitorEventId == pool.Id).ToList();

                foreach (var stage in stages)
                {
                    var stageResults = context.Results.Where(r => r.StageId == stage.Id).ToList();
                    var scorePerPick = pickList.ToDictionary(p => p.Id, p => 0);

                    foreach (var result in stageResults)
                    {
                        foreach (var pick in pickList)
                        {
                            if (pick.CompetitorsInEventId == result.CompetitorInEventId)
                            {
                                var pts = context.ConfigurationItems.First(ci => ci.Id == result.ConfigurationItemId).Score;
                                scorePerPick[pick.Id] = pts;
                            }
                        }
                    }

                    int stageTotal = scorePerPick.Values.Sum();
                    int cumulativeTotal = context.DeelnemerScores.Where(ds => ds.GameCompetitorEventId == pool.Id).Sum(ds => ds.LaatsteStageScore) + stageTotal;

                    context.DeelnemerScores.Add(new DeelnemerScore
                    {
                        GameCompetitorEventId = pool.Id,
                        LaatsteStageId = stage.Id,
                        TotalScore = cumulativeTotal,
                        LaatsteStageScore = stageTotal,
                        LastUpdated = DateTime.UtcNow
                    });

                    foreach (var pick in pickList)
                    {
                        context.DeelnemerPickScores.Add(new DeelnemerPickScore
                        {
                            GameCompetitorEventPickId = pick.Id,
                            //StageId = stage.Id,
                            TotalScore = scorePerPick[pick.Id],
                            LastUpdate = DateTime.UtcNow
                        });
                    }
                }
            }

            await AddScores(poolTop20, cieListTop20, stagesTop20, top20Config, Top20Points);
            await AddScores(poolCustom15, cieListCustom15, stagesCustom15, custom15Config, Custom15Points);
            await context.SaveChangesAsync();

            Console.WriteLine("SeedData: 2 events, 2 configs, 2 pools klaar.");
        }
    }
}
