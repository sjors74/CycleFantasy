using CycleManager.Domain.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class GameCompetitorEventPickRepository : GenericRepository<GameCompetitorEventPick>, IGameCompetitorEventPickRepository
    {
        public GameCompetitorEventPickRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public IQueryable<GameCompetitorEventPick> GetCompetitorEventPicksByEventId(int eventId)
        {
            var picks = context.GameCompetitorEventPicks
                .Include(c => c.CompetitorsInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                       .ThenInclude(a => a.Competitor)
                .Include(g => g.GameCompetitorEvent)
                    .ThenInclude(b => b.User)
                .Where(c => c.CompetitorsInEvent.EventId.Equals(eventId));
            return picks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GameCompetitorEventPick>> GetCompetitorEventPicksById(int id)
        {
            var picks = await context.GameCompetitorEventPicks
                .Include(g => g.GameCompetitorEvent)
                    .ThenInclude(b => b.User)
                .Include(c => c.CompetitorsInEvent)
                    .ThenInclude(c => c.CompetitorInTeam)
                        .ThenInclude(cie => cie.Competitor)
                            .ThenInclude(c => c.Country)
                .Include(c => c.CompetitorsInEvent)
                        .ThenInclude(c => c.CompetitorInTeam)
                            .ThenInclude(cit => cit.Team)
                .Include(c => c.CompetitorsInEvent)
                    .ThenInclude(cie => cie.Event)
                .Where(c => c.GameCompetitorEvent.Id == id)
                .OrderBy(c => c.CompetitorsInEvent.EventNumber)
                .ToListAsync();

            return picks;
        }

        public async Task CreateGamePicksAsync(List<GameCompetitorEventPick> picks)
        {
            var gameCompetitorEventId = picks.First().GameCompetitorEventId;

            // Maak een HashSet van actuele picks (vanuit frontend)
            var incomingPickIds = picks
                .Select(p => p.CompetitorsInEventId)
                .ToHashSet();

            // Haal bestaande picks van deze deelnemer op
            var existingPicks = await context.GameCompetitorEventPicks
                .Where(p => p.GameCompetitorEventId == gameCompetitorEventId)
                .ToListAsync();

            // Vind de picks die verwijderd moeten worden
            var toDelete = existingPicks
                .Where(p => !incomingPickIds.Contains(p.CompetitorsInEventId))
                .ToList();

            // Vind de picks die nieuw zijn
            var existingPickIds = existingPicks
                .Select(p => p.CompetitorsInEventId)
                .ToHashSet();

            var newPicks = picks
                .Where(p => !existingPickIds.Contains(p.CompetitorsInEventId))
                .ToList();

            // Pas wijzigingen toe
            context.GameCompetitorEventPicks.RemoveRange(toDelete);
            await context.GameCompetitorEventPicks.AddRangeAsync(newPicks);

            try
            {
                var changes = await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Er is een fout opgetreden: {ex.Message}");
            }
        }

        public async Task RemovePickFromEvent(int id)
        {
            var pick = await context.GameCompetitorEventPicks.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (pick != null)
            {
                context.GameCompetitorEventPicks.Remove(pick);
            }
        }
    }
}
