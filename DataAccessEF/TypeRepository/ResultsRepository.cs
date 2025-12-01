using CycleManager.Domain.Dto;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

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
            var result =
                (from ds in context.DeelnemerPickScores
                 join gcp in context.GameCompetitorEventPicks
                     on ds.GameCompetitorEventPickId equals gcp.Id
                 join s in context.Stages
                     on ds.StageId equals s.Id
                 where s.EventId == eventId
                    && gcp.CompetitorsInEventId == competitorInEventId
                 select new
                 {
                     ds.StageId,
                     gcp.CompetitorsInEventId,
                     ds.Score
                 })
                .Distinct()
                .GroupBy(x => x.CompetitorsInEventId)
                .Select(g => new CompetitorScoreDto
                {
                    CompetitorInEventId = g.Key,
                    TotalScore = g.Sum(x => x.Score)
                })
                .FirstOrDefault();

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

        public async Task<List<EtappeUitslagDto>?> GetEtappeUitslag(int stageId)
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
                noResultsItem.NoScoreDescription = stage.NoScoreDescription;
                noResultsItem.NoScore = true;
                emptyListOfResults.Add(noResultsItem);
                return emptyListOfResults;
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
                    .ThenInclude(cie => cie.CompetitorInTeam.Competitor) // één pad
                .Include(r => r.ConfigurationItem)
                .ToListAsync();

            var top15 = configItems.Select(ci =>
            {
                var result = results.FirstOrDefault(r => r.ConfigurationItem.Position == ci.Position);
                if (result == null || result.CompetitorInEvent?.CompetitorInTeam?.Competitor == null)
                    return null;

                var competitor = result.CompetitorInEvent.CompetitorInTeam.Competitor;
                var team = competitor.CompetitorInTeams.FirstOrDefault()?.Team;
                return new EtappeUitslagDto
                {
                    Positie = ci.Position,
                    CompetitorName = $"{competitor.FirstName} {competitor.LastName}",
                    TeamName = team?.CurrentTeamName,
                    Score = ci.Score
                };
            })
            .Where(r => r != null)
            .ToList();

            return top15;
        }

        //Methodes voor de Manager
        public async Task<Stage?> GetStageByIdAsync(int stageId)
        {
            return await context.Stages
                .AsNoTracking()
                .Include(s => s.Event)
                .ThenInclude(e => e.Configuration)
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
            // --- 1. Haal event inclusief configuratie, stages, deelnemers en picks ---
            var ev = await context.Events
                .Include(e => e.Configuration)
                    .ThenInclude(c => c.ConfigurationItems)
                .Include(e => e.Stages)
                .Include(e => e.GameCompetitorEvents)
                    .ThenInclude(gce => gce.Renners) // Picks
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null)
                throw new InvalidOperationException($"Event {eventId} not found");

            var newConfigItems = ev.Configuration.ConfigurationItems
                .OrderBy(ci => ci.Position)
                .ToList();

            // --- 2. Haal alle resultaten voor dit event ---
            var allResults = await context.Results
                .Where(r => r.Stage.EventId == eventId)
                .ToListAsync();

            // --- 3. Update ConfigurationItemId in Results op basis van positie ---
            foreach (var result in allResults)
            {
                var oldCi = await context.ConfigurationItems
                    .FirstOrDefaultAsync(ci => ci.Id == result.ConfigurationItemId);

                if (oldCi == null)
                    throw new InvalidOperationException($"Old ConfigurationItem {result.ConfigurationItemId} not found");

                var newCi = newConfigItems.FirstOrDefault(ci => ci.Position == oldCi.Position);

                result.ConfigurationItemId = newCi?.Id; // null als positie buiten nieuwe configuratie
            }

            await context.SaveChangesAsync();

            // --- 4. Verwijder oude pick-scores en deelnemer-scores ---
            var allPickIds = ev.GameCompetitorEvents
                .SelectMany(gce => gce.Renners)
                .Select(p => p.Id)
                .ToList();

            var oldPickScores = await context.DeelnemerPickScores
                .Where(ps => allPickIds.Contains(ps.GameCompetitorEventPickId))
                .ToListAsync();

            context.DeelnemerPickScores.RemoveRange(oldPickScores);

            var oldScores = await context.DeelnemerScores
                .Where(ds => ds.Stage.EventId == eventId)
                .ToListAsync();

            context.DeelnemerScores.RemoveRange(oldScores);
            await context.SaveChangesAsync();

            // --- 5. Bereken nieuwe scores ---
            foreach (var gce in ev.GameCompetitorEvents)
            {
                int totalScore = 0;
                int lastStageScore = 0;
                int? lastStageId = null;

                foreach (var pick in gce.Renners) // pick = GameCompetitorEventPick
                {
                    int pickTotal = 0;
                    int pickLastStageScore = 0;
                    int? pickLastStageId = null;

                    var pickResults = allResults
                        .Where(r => r.CompetitorInEventId == pick.CompetitorsInEventId)
                        .OrderBy(r => r.StageId);

                    foreach (var r in pickResults)
                    {
                        if (r.ConfigurationItemId == null)
                            continue;

                        var ci = newConfigItems.First(ci => ci.Id == r.ConfigurationItemId);
                        pickTotal += ci.Score;

                        // laatste stage
                        pickLastStageScore = ci.Score;
                        pickLastStageId = r.StageId;
                    }

                    // Nieuwe pick-score toevoegen
                    var ps = new DeelnemerPickScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventPickId = pick.Id,
                        StageId = pickLastStageId,  // nullable
                        Score = pickTotal,
                        LastUpdate = DateTime.UtcNow
                    };
                    context.DeelnemerPickScores.Add(ps);

                    // total & laatste stage voor deelnemer
                    totalScore += pickTotal;
                    if (pickLastStageId.HasValue &&
                        (!lastStageId.HasValue || pickLastStageId > lastStageId))
                    {
                        lastStageId = pickLastStageId;
                        lastStageScore = pickLastStageScore;
                    }
                }

                // Nieuwe deelnemer-score toevoegen
                var ds = new DeelnemerScore
                {
                    GameCompetitorEventId = gce.Id,
                    StageId = lastStageId,       // nullable
                    TotalScore = totalScore,
                    LaatsteScore = lastStageScore,
                    LastUpdated = DateTime.UtcNow
                };
                context.DeelnemerScores.Add(ds);
            }

            await context.SaveChangesAsync();
        }

    }
}
