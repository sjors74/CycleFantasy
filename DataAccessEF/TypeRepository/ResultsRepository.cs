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
            // haal alle picks van deze deelnemer in dit event
            var pickIds = await context.GameCompetitorEventPicks
                .Where(p => p.CompetitorsInEventId == competitorInEventId
                         && p.GameCompetitorEvent.EventId == eventId)
                .Select(p => p.Id)
                .ToListAsync();

            if (!pickIds.Any())
                return null;

            // cumulatieve scores van deze picks
            var totalScore = await context.DeelnemerPickScores
                .Where(ds => pickIds.Contains(ds.GameCompetitorEventPickId))
                .SumAsync(ds => ds.TotalScore);

            return new CompetitorScoreDto
            {
                CompetitorInEventId = competitorInEventId,
                TotalScore = totalScore
            };
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

            var details = picks.Select(p =>
            {
                var rennerResults = results
                    .Where(r => r.CompetitorInEventId == p.CompetitorsInEventId)
                    .ToList();

                return new PickDetailDto
                {
                    CompetitorInEventId = p.CompetitorsInEventId,
                    CompetitorName = p.CompetitorName,
                    TotalScore = rennerResults.Sum(r => r.Score),
                    LastScore = rennerResults.Where(r => r.StageId == lastStageId).Sum(r => r.Score)
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

            var configItems = ev.Configuration.ConfigurationItems
                .OrderBy(ci => ci.Position)
                .ToList();

            var stages = ev.Stages
                .OrderBy(s => s.Id)
                .ToList();

            // --- 2. Haal alle resultaten voor dit event ---
            var allResults = await context.Results
                .Where(r => r.Stage.EventId == eventId)
                .Include(r => r.ConfigurationItem)
                .ToListAsync();

            // --- 3. Update ConfigurationItemId in Results op basis van positie ---
            foreach (var result in allResults)
            {
                if(result.ConfigurationItemId == null)
                    continue;


                var oldCi = result.ConfigurationItem;
                var newCi = configItems.FirstOrDefault(ci => ci.Position == oldCi.Position);
                result.ConfigurationItemId = newCi?.Id;
            }

            await context.SaveChangesAsync();

            // --- 4. Verwijder oude pick-scores en deelnemer-scores ---
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

            // --- 5. Bereken nieuwe scores ---
            // prepare accumulators
            var pickTotals = pickIds.ToDictionary(pid => pid, _ => 0);  // pickId → total score
            var deelnemerTotals = deelnemerIds.ToDictionary(gid => gid, _ => 0); // gceId → total score

            // results per stage
            var resultsByStage = allResults
                .GroupBy(r => r.StageId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(r => r.ConfigurationItemId != null)
                          .ToDictionary(
                              x => x.CompetitorInEventId,
                              x => configItems.First(ci => ci.Id == x.ConfigurationItemId).Score
                          )
                );

            foreach (var stage in stages)
            {
                var stageId = stage.Id;
                var stageResults = resultsByStage.ContainsKey(stageId)
                    ? resultsByStage[stageId]
                    : new Dictionary<int, int>();

                foreach (var gce in ev.GameCompetitorEvents)
                {
                    int stageScoreForDeelnemer = 0;

                    foreach (var pick in gce.Renners)
                    {
                        int pickScore = stageResults.TryGetValue(pick.CompetitorsInEventId, out var score)
                            ? score
                            : 0;

                        pickTotals[pick.Id] += pickScore;

                        // stage-pickscore opslaan
                        context.DeelnemerStagePickScores.Add(new DeelnemerStagePickScore
                        {
                            Id = Guid.NewGuid(),
                            GameCompetitorEventPickId = pick.Id,
                            StageId = stage.Id,
                            Score = pickScore,
                            LastUpdated = DateTime.UtcNow
                        });

                        stageScoreForDeelnemer += pickScore;
                    }

                    // opslaan deelnemer score voor deze stage (snapshot)
                    context.DeelnemerStageScores.Add(new DeelnemerStageScore
                    {
                        Id = Guid.NewGuid(),
                        GameCompetitorEventId = gce.Id,
                        StageId = stage.Id,
                        Score = stageScoreForDeelnemer,
                        LastUpdated = DateTime.UtcNow
                    });

                    // cumulatief deelnemer totaal
                    deelnemerTotals[gce.Id] += stageScoreForDeelnemer;
                }
            }

            // ##########################################################
            // ### 6. SLA CUMULATIEVE PICK SCORES OP ###
            // ##########################################################

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

            // ##########################################################
            // ### 7. SLA CUMULATIEVE DEELNEMER SCORES OP ###
            // ##########################################################

            foreach (var gce in ev.GameCompetitorEvents)
            {
                var last = context.DeelnemerStageScores
                    .Where(s => s.GameCompetitorEventId == gce.Id)
                    .OrderByDescending(s => s.StageId)
                    .First();

                context.DeelnemerScores.Add(new DeelnemerScore
                {
                    Id = Guid.NewGuid(),
                    GameCompetitorEventId = gce.Id,
                    TotalScore = deelnemerTotals[gce.Id],
                    LaatsteStageId = last.StageId,
                    LaatsteStageScore = last.Score,
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
    }
}
