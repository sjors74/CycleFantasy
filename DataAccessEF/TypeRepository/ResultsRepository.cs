using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataAccessEF.TypeRepository
{
    public class ResultsRepository : GenericRepository<Result>, IResultsRepository
    {
        public ResultsRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Result>> GetResultsByEventId(int eventId)
        {
            var results = await context.Results
                .Include(c => c.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                 .Include(c => c.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(c => c.Competitor)
                .Include(s => s.Stage)
                .Include(r => r.ConfigurationItem)
                .Where(r => r.Stage.EventId == eventId)
                .OrderBy(r => r.ConfigurationItem.Position)
                .ToListAsync();
            return results;
        }

        public async Task<CompetitorScoreDto?> GetCompetitorResultsByEventId(int eventId, int competitorInEventId)
        {
            // haal alle picks van deze deelnemer in dit event
            var pickIds = await context.GameCompetitorEventPicks
                .Where(p => p.CompetitorsInEventId == competitorInEventId
                         && p.GameCompetitorEvent.EventId == eventId)
                .Select(p => p.Id)
                .ToListAsync();

            if (!pickIds.Any())
                return null;

            // normale scores over alle etappes van alle picks
            var normalScore = await context.DeelnemerStagePickScores
                .Where(x => pickIds.Contains(x.GameCompetitorEventPickId))
                .SumAsync(x => x.Score);


            // speciale scores over alle etappes van alle picks
            var specialScore = await context.DeelnemerStagePickSpecialScores
                .Where(x => pickIds.Contains(x.GameCompetitorEventPickId))
                .SumAsync(x => x.Score);

            return new CompetitorScoreDto
            {
                CompetitorInEventId = competitorInEventId,
                NormalScore = normalScore,
                SpecialScore = specialScore
            };
        }

        public async Task<List<CompetitorScoreDto>> GetCompetitorResultsForEvent(int eventId)
        {
            var normalScores = await (
                from p in context.GameCompetitorEventPicks
                join s in context.DeelnemerStagePickScores
                    on p.Id equals s.GameCompetitorEventPickId
                where p.GameCompetitorEvent.EventId == eventId
                group s by p.CompetitorsInEventId into g
                select new
                {
                    CompetitorInEventId = g.Key,
                    NormalScore = g.Sum(x => x.Score)
                })
                .ToListAsync();


            var specialScores = await (
                from p in context.GameCompetitorEventPicks
                join s in context.DeelnemerStagePickSpecialScores
                    on p.Id equals s.GameCompetitorEventPickId
                where p.GameCompetitorEvent.EventId == eventId
                group s by p.CompetitorsInEventId into g
                select new
                {
                    CompetitorInEventId = g.Key,
                    SpecialScore = g.Sum(x => x.Score)
                })
                .ToListAsync();


            var result = normalScores
                .Select(x => new CompetitorScoreDto
                {
                    CompetitorInEventId = x.CompetitorInEventId,
                    NormalScore = x.NormalScore,
                    SpecialScore = specialScores
                        .Where(s => s.CompetitorInEventId == x.CompetitorInEventId)
                        .Select(s => s.SpecialScore)
                        .FirstOrDefault()
                })
                .ToList();


            // deelnemers die alleen specials hebben toegevoegd
            foreach (var special in specialScores.Where(s =>
                !result.Any(r => r.CompetitorInEventId == s.CompetitorInEventId)))
            {
                result.Add(new CompetitorScoreDto
                {
                    CompetitorInEventId = special.CompetitorInEventId,
                    NormalScore = 0,
                    SpecialScore = special.SpecialScore
                });
            }

            return result;
        }

        public async Task<int> GetCompetitorLatestScore(int eventId, int competitorInEventId)
        {
            var configItems = await context.ConfigurationItems.ToListAsync();
            var configDict = configItems.ToDictionary(ci => ci.Id, ci => ci.Score);

            int? laatsteVerredenStageId = await context.Results
                .Where(r => r.CompetitorInEvent.EventId == eventId)
                .Select(r => r.StageId)
                .Distinct()
                .OrderByDescending(id => id)
                .FirstOrDefaultAsync();

            if (laatsteVerredenStageId == 0)
            {
                return 0;
            }

            var results = await context.Results
                .Where(r => r.StageId == laatsteVerredenStageId && r.CompetitorInEventId == competitorInEventId)
                .ToListAsync();

            int score = results.Sum(r => r.ConfigurationItemId.HasValue && configDict.TryGetValue(r.ConfigurationItemId.Value, out var s) ? s : 0);
            return score;
        }

        public async Task<int> GetResultsByStageId(int stageId)
        {
            return await context.Results
                .CountAsync(r => r.StageId == stageId);
        }

        public async Task<EtappeResultaatDto>? GetEtappeUitslag(int stageId)
        {
            var stage = await context.Stages
                .Include(s => s.Event)
                    .ThenInclude(e => e.Configuration)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
            {
                return null;
            }
            ;

            if (stage.NoScore)
            {
                var emptyListOfResults = new List<EtappeUitslagDto>();
                var noResultsItem = new EtappeUitslagDto();
                noResultsItem.NoScoreDescription = stage.NoScoreDescription ?? string.Empty;
                noResultsItem.NoScore = true;
                emptyListOfResults.Add(noResultsItem);
                return new EtappeResultaatDto
                {
                    Uitslag = emptyListOfResults,
                    Specials = new List<EtappeSpecialDto>()
                };
            }

            var configItems = await context.ConfigurationItems
                .AsNoTracking()
                .Where(ci => ci.ConfigurationId == stage.Event.ConfigurationId)
                .OrderBy(ci => ci.Position)
                .ToListAsync();

            var results = await context.Results
                .AsNoTracking()
                .Where(r => r.StageId == stageId)
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(cie => cie.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(cie => cie.CompetitorInTeam.Competitor)
                .Include(r => r.ConfigurationItem)
                .ToListAsync();

            var resultLookup = results
                .Where(r => r.ConfigurationItemId.HasValue)
                .ToDictionary(
                    r => r.ConfigurationItemId!.Value,
                    r => r
                );

            var configurationSpecialItems = await context.ConfigurationItemSpecials
                .AsNoTracking()
                .Where(c => c.ConfigurationId == stage.Event.ConfigurationId)
                .OrderBy(c => c.Question)
                .ToListAsync();

            var specialResults = await context.SpecialResults
                .AsNoTracking()
                .Where(r => r.StageId == stageId)
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(cie => cie.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(cie => cie.CompetitorInTeam)
                        .ThenInclude(cit => cit.Competitor)
                .Include(r => r.Special)
                .ToListAsync();

            var specialLookup = specialResults
                .Where(r => r.SpecialId.HasValue)
                .ToDictionary(
                    r => r.SpecialId!.Value,
                    r => r
                );
            var uitslag = configItems.Select(ci =>
            {
                if (!resultLookup.TryGetValue(ci.Id, out var result))
                    return null;    

                var competitorInTeam = result.CompetitorInEvent?.CompetitorInTeam;
                var competitor = competitorInTeam?.Competitor;
                var team = competitor?.CompetitorInTeams?.FirstOrDefault()?.Team;

                if (competitor == null)
                    return null;

                return new EtappeUitslagDto
                {
                    Positie = ci.Position,
                    CompetitorName = $"{competitor.FirstName} {competitor.LastName}",
                    TeamName = team?.CurrentTeamName ?? string.Empty,
                    Score = ci.Score
                };
            })
            .Where(r => r != null)
            .Cast<EtappeUitslagDto>()
            .ToList();

            var specials = configurationSpecialItems.Select(item =>
            {
                if (!specialLookup.TryGetValue(item.Id, out var result))
                    return null;

                var competitorInTeam = result.CompetitorInEvent?.CompetitorInTeam;
                var competitor = competitorInTeam?.Competitor;
                var team = competitor?.CompetitorInTeams?.FirstOrDefault()?.Team;

                if (competitor == null)
                    return null;

                return new EtappeSpecialDto
                {
                    Name = item.Question.ToString(),
                    Color = item.Color,
                    CompetitorName = $"{competitor.FirstName} {competitor.LastName}",
                    TeamName = team?.CurrentTeamName ?? string.Empty,
                    Score = item.Score

                };
            })
            .Where(x => x != null)
            .Cast<EtappeSpecialDto>()
            .ToList();

            return new EtappeResultaatDto
            {
                Uitslag = uitslag,
                Specials = specials
            };
        }

        public async Task<List<PickDetailDto>> GetPickDetailsAsync(int eventId, int gameCompetitorEventId)
        {
            var lastStageId = await context.Results
                .Where(r => r.Stage.EventId == eventId)
                .MaxAsync(r => (int?)r.StageId);

            if (!lastStageId.HasValue)
                lastStageId = null;

            var picks = await context.GameCompetitorEventPicks
                .Where(p =>
                    p.GameCompetitorEventId == gameCompetitorEventId &&
                    p.GameCompetitorEvent.EventId == eventId)
                .Select(p => new
                {
                    p.CompetitorsInEventId,
                    CompetitorName = p.CompetitorsInEvent
                        .CompetitorInTeam
                        .Competitor
                        .CompetitorName
                })
                .ToListAsync();
            if (!picks.Any())
                return new List<PickDetailDto>();

            var competitorIds = picks.Select(p => p.CompetitorsInEventId).ToList();

            var results = await context.Results
                .Where(r =>
                    competitorIds.Contains(r.CompetitorInEventId) &&
                    r.Stage.EventId == eventId &&
                    r.ConfigurationItemId != null)
                .Select(r => new
                {
                    r.CompetitorInEventId,
                    r.StageId,
                    Score = r.ConfigurationItem.Score
                })
                .ToListAsync();

            var specialResults = await context.SpecialResults
                .Where(r =>
                    competitorIds.Contains(r.CompetitorInEventId) &&
                    r.Stage.EventId == eventId)
                .Select(r => new
                {
                    r.CompetitorInEventId,
                    r.StageId,
                    Score = r.Special.Score,
                    Name = r.Special.Question.ToString()
                })
                .ToListAsync();

            var details = picks.Select(p =>
            {
                var rennerResults = results
                    .Where(r => r.CompetitorInEventId == p.CompetitorsInEventId)
                    .ToList();

                var normalScore = rennerResults.Sum(r => r.Score);

                var rennerSpecials = specialResults
                    .Where(r => r.CompetitorInEventId == p.CompetitorsInEventId)
                    .ToList();

                var specialScore = rennerSpecials.Sum(r => r.Score);

                return new PickDetailDto
                {
                    CompetitorInEventId = p.CompetitorsInEventId,
                    CompetitorName = p.CompetitorName,

                    NormalScore = normalScore,
                    SpecialScore = specialScore,
                    TotalScore = normalScore + specialScore,
                    Specials = rennerSpecials.Select(s => new SpecialDetailDto
                    {
                        Name = s.Name,
                        Score = s.Score
                    }).ToList(),
                    LastScore =
                        rennerResults
                            .Where(r => r.StageId == lastStageId)
                            .Sum(r => r.Score)
                        +
                        rennerSpecials
                            .Where(r => r.StageId == lastStageId)
                            .Sum(r => r.Score)
                };
            })
            .OrderByDescending(d => d.TotalScore)
            .ToList();

            return details;
        }


        //Methodes voor de Manager
        public async Task<Stage?> GetStageByIdAsync(int stageId)
        {
            return await context.Stages
                .AsNoTracking()
                .Include(s => s.Event)
                .ThenInclude(e => e.Configuration)
                .ThenInclude(c => c.ConfigurationItems)
                .FirstOrDefaultAsync(s => s.Id == stageId);
        }

        public async Task<List<Result>> GetResultsByStageAsync(int stageId)
        {
            return await context.Results
                .AsNoTracking()
                .Where(r => r.StageId == stageId)
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(r => r.CompetitorInTeam)
                        .ThenInclude(cie => cie.Competitor)
                .Include(r => r.ConfigurationItem)
                .ToListAsync();
        }

        public async Task<List<CompetitorsInEvent>> GetCompetitorsInEventAsync(int eventId)
        {
            return await context.CompetitorsInEvent
                .AsNoTracking()
                .Where(c => c.EventId == eventId && !c.OutOfCompetition)
                .Include(c => c.CompetitorInTeam)
                    .ThenInclude(c => c.Competitor)
                .ToListAsync();
        }

        public async Task<List<ConfigurationItem>> GetConfigurationItemsByConfigAsync(int configId)
        {
            return await context.ConfigurationItems
                .AsNoTracking()
                .Where(ci => ci.ConfigurationId == configId)
                .OrderBy(ci => ci.Position)
                .ToListAsync();
        }

        public async Task AddResultsAsync(IEnumerable<Result> results)
        {
            context.Results.AddRange(results);
            await context.SaveChangesAsync();
        }

        public async Task<Result?> GetResultByIdAsync(int id)
        {
            return await context.Results
                .Include(r => r.CompetitorInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(r => r.Competitor)
                .Include(r => r.Stage)
                .Include(r => r.ConfigurationItem)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task DeleteResultAsync(Result result)
        {
            var existing = await context.Results.FindAsync(result.Id);
            if (existing != null)
            {
                context.Results.Remove(result);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ResultExistsAsync(int id)
        {
            return await context.Results.AnyAsync(e => e.Id == id);
        }

        public string GetCompetitorFullName(int competitorId)
        {
            var competitor = context.Competitors.FirstOrDefault(c => c.CompetitorId == competitorId);
            return competitor != null ? $"{competitor.FirstName} {competitor.LastName}" : string.Empty;
        }

        /// <summary>
        /// Recalculate all scores for an event based on current ConfigurationItems.
        /// Updates Results, DeelnemerScores, and DeelnemerPickScores.
        /// </summary>
        public async Task RecalculateEventScoresAsync(int eventId)
        {
            // --- 1. EVENT LOAD ---
            var ev = await context.Events
                .Include(e => e.Configuration)
                    .ThenInclude(c => c.ConfigurationItems)
                .Include(e => e.Configuration)
                    .ThenInclude(c => c.Specials)
                .Include(e => e.Stages)
                .Include(e => e.GameCompetitorEvents)
                    .ThenInclude(gce => gce.Renners)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null)
                throw new InvalidOperationException($"Event {eventId} not found");

            // --- CONFIG LOOKUPS (FIX 3) ---
            var configScoreById = ev.Configuration.ConfigurationItems
                .ToDictionary(x => x.Id, x => x.Score);

            var specialScoreById = ev.Configuration.Specials
                .ToDictionary(x => x.Id, x => x.Score);

            var configItems = ev.Configuration.ConfigurationItems.ToList();
            var specialConfigItems = ev.Configuration.Specials.ToList();

            var stages = ev.Stages
                .OrderBy(s => s.Id)
                .ToList();

            // --- 2. RESULTS ---
            var normalResults = await context.Results
                .Where(r => r.Stage.EventId == eventId)
                .Include(r => r.ConfigurationItem)
                .ToListAsync();

            var specialResults = await context.SpecialResults
                .Where(r => r.Stage.EventId == eventId)
                .Include(r => r.Special)
                .ToListAsync();

            // --- 3. CONFIG UPDATE ---
            foreach (var result in normalResults)
            {
                if (result.ConfigurationItemId == null)
                    continue;

                var oldCi = result.ConfigurationItem;
                var newCi = configItems.FirstOrDefault(ci => ci.Position == oldCi.Position);

                result.ConfigurationItemId = newCi?.Id;
            }

            await context.SaveChangesAsync();

            // --- 4. CLEAN OLD DATA ---
            var stageIds = ev.Stages.Select(s => s.Id).ToList();
            var deelnemerIds = ev.GameCompetitorEvents.Select(g => g.Id).ToList();
            var pickIds = ev.GameCompetitorEvents.SelectMany(g => g.Renners).Select(p => p.Id).ToList();

            context.DeelnemerStagePickScores.RemoveRange(
                context.DeelnemerStagePickScores.Where(x => stageIds.Contains(x.StageId))
            );

            context.DeelnemerStageScores.RemoveRange(
                context.DeelnemerStageScores.Where(x => stageIds.Contains(x.StageId))
            );

            context.DeelnemerPickScores.RemoveRange(
                context.DeelnemerPickScores.Where(x => deelnemerIds.Contains(x.Pick.GameCompetitorEventId))
            );

            context.DeelnemerScores.RemoveRange(
                context.DeelnemerScores.Where(s => deelnemerIds.Contains(s.GameCompetitorEventId))
            );

            await context.SaveChangesAsync();

            // --- 5. ACCUMULATORS ---
            var pickTotals = pickIds.ToDictionary(pid => pid, _ => 0);
            var deelnemerTotals = deelnemerIds.ToDictionary(gid => gid, _ => 0);

            // --- 6. BUILD LOOKUPS (FIX 3) ---
            var normalResultsByStage = normalResults
                .Where(r => r.ConfigurationItemId != null)
                .GroupBy(r => r.StageId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(
                        x => x.CompetitorInEventId,
                        x => configScoreById[x.ConfigurationItemId!.Value]
                    )
                );

            var specialResultsByStage = specialResults
                .GroupBy(r => r.StageId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(
                        x => x.CompetitorInEventId,
                        x => specialScoreById[x.SpecialId!.Value]
                    )
                );

            // --- 7. MAIN LOOP ---
            foreach (var stage in stages)
            {
                var stageId = stage.Id;

                var normalStage = normalResultsByStage.ContainsKey(stageId)
                    ? normalResultsByStage[stageId]
                    : new Dictionary<int, int>();

                var specialStage = specialResultsByStage.ContainsKey(stageId)
                    ? specialResultsByStage[stageId]
                    : new Dictionary<int, int>();

                foreach (var gce in ev.GameCompetitorEvents)
                {
                    int stageTotal = 0;

                    foreach (var pick in gce.Renners)
                    {
                        int normalScore = normalStage.TryGetValue(pick.CompetitorsInEventId, out var n) ? n : 0;
                        int specialScore = specialStage.TryGetValue(pick.CompetitorsInEventId, out var s) ? s : 0;

                        int total = normalScore + specialScore;

                        // pick accumulator
                        pickTotals[pick.Id] += total;

                        // stage pick score (NORMAL)
                        context.DeelnemerStagePickScores.Add(new DeelnemerStagePickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stage.Id,
                            Score = normalScore,
                            LastUpdated = DateTime.UtcNow
                        });

                        // stage pick special score
                        context.DeelnemerStagePickSpecialScores.Add(new DeelnemerStagePickSpecialScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stage.Id,
                            Score = specialScore,
                            LastUpdated = DateTime.UtcNow
                        });

                        stageTotal += total;
                    }

                    // stage snapshot per deelnemer
                    context.DeelnemerStageScores.Add(new DeelnemerStageScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = gce.Id,
                        StageId = stage.Id,
                        Score = stageTotal,
                        LastUpdated = DateTime.UtcNow
                    });

                    deelnemerTotals[gce.Id] += stageTotal;
                }
            }

            // --- 8. PICK TOTALS ---
            foreach (var pickId in pickTotals.Keys)
            {
                context.DeelnemerPickScores.Add(new DeelnemerPickScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventPickId = pickId,
                    TotalScore = pickTotals[pickId],
                    LastUpdate = DateTime.UtcNow
                });
            }

            // --- 9. DEELNEMER TOTALS ---
            foreach (var gce in ev.GameCompetitorEvents)
            {
                var last = context.DeelnemerStageScores
                    .Where(s => s.GameCompetitorEventId == gce.Id)
                    .OrderByDescending(s => s.StageId)
                    .FirstOrDefault();

                context.DeelnemerScores.Add(new DeelnemerScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventId = gce.Id,
                    TotalScore = deelnemerTotals[gce.Id],
                    LaatsteStageId = last?.StageId ?? 0,
                    LaatsteStageScore = last?.Score ?? 0,
                    LastUpdated = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<DeelnemerScore>> GetTotalScoresByEventIdAsync(int eventId)
        {
            var gceIds = await context.GameCompetitorsEvent
                .Where(gce => gce.EventId == eventId)
                .Select(gce => gce.Id)
                .ToListAsync();

            return await context.DeelnemerScores
                .Where(ds => gceIds.Contains(ds.GameCompetitorEventId))
                .ToListAsync();
        }

        public async Task SyncResultsAsync(int stageId, IEnumerable<Result> results, IEnumerable<SpecialResult> specialResults)
        {
            // ========================================
            // NORMAL RESULTS
            // ========================================

            var existingResults = await context.Results
                .Where(r => r.StageId == stageId)
                .ToListAsync();

            // verwijderen
            var resultsToRemove = existingResults
                .Where(db =>
                    !results.Any(r =>
                        r.CompetitorInEventId == db.CompetitorInEventId))
                .ToList();

            context.Results.RemoveRange(resultsToRemove);

            // toevoegen
            var resultsToAdd = results
                .Where(r =>
                    !existingResults.Any(db =>
                        db.CompetitorInEventId == r.CompetitorInEventId))
                .ToList();

            context.Results.AddRange(resultsToAdd);

            // ========================================
            // SPECIAL RESULTS
            // ========================================

            var existingSpecialResults = await context.SpecialResults
                .Where(r => r.StageId == stageId)
                .ToListAsync();

            // verwijderen
            var specialsToRemove = existingSpecialResults
                .Where(db =>
                    !specialResults.Any(s =>
                        s.SpecialId == db.SpecialId))
                .ToList();

            context.SpecialResults.RemoveRange(specialsToRemove);

            // toevoegen
            var specialsToAdd = specialResults
                .Where(s =>
                    !existingSpecialResults.Any(db =>
                        db.SpecialId == s.SpecialId))
                .ToList();

            context.SpecialResults.AddRange(specialsToAdd);

            await context.SaveChangesAsync();
        }

        public async Task<List<DeelnemerScoreBreakdown>> GetScoreBreakdownByEventIdAsync(int eventId)
        {
            var deelnemers = await context.GameCompetitorsEvent
                .Where(x => x.EventId == eventId)
                .Select(x => x.Id)
                .ToListAsync();


            var picks = await context.GameCompetitorEventPicks
                .Where(p => deelnemers.Contains(p.GameCompetitorEventId))
                .Select(p => new
                {
                    DeelnemerId = p.GameCompetitorEventId,
                    PickId = p.Id
                })
                .ToListAsync();


            var pickLookup = picks.ToDictionary(
                x => x.PickId,
                x => x.DeelnemerId);


            var pickIds = picks
                .Select(x => x.PickId)
                .ToList();


            var normalScores = await context.DeelnemerStagePickScores
                .Where(x => pickIds.Contains(x.GameCompetitorEventPickId))
                .ToListAsync();


            var specialScores = await context.DeelnemerStagePickSpecialScores
                .Where(x => pickIds.Contains(x.GameCompetitorEventPickId))
                .ToListAsync();


            var normalByDeelnemer = normalScores
                .GroupBy(x => pickLookup[x.GameCompetitorEventPickId])
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Score));


            var specialByDeelnemer = specialScores
                .GroupBy(x => pickLookup[x.GameCompetitorEventPickId])
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Score));


            return deelnemers.Select(id => new DeelnemerScoreBreakdown
            {
                GameCompetitorEventId = id,

                NormalPoints = normalByDeelnemer.TryGetValue(id, out var normal)
                    ? normal
                    : 0,

                SpecialPoints = specialByDeelnemer.TryGetValue(id, out var special)
                    ? special
                    : 0

            }).ToList();
        }
    }
}
