using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorsInEventRepository: GenericRepository<CompetitorsInEvent>, ICompetitorsInEventRepository
    {
        public CompetitorsInEventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CompetitorsInEvent> GetById(int id)
        {
            var competitorInEvent = await context.CompetitorsInEvent
                .Include(c => c.Competitor)
                    .ThenInclude(c => c.Team)
                .Include(c => c.Competitor)
                    .ThenInclude(c => c.Country)
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            return competitorInEvent;
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetCompetitors(int eventId)
        {
            return await context.CompetitorsInEvent
                .Where(c => c.EventId == eventId)
                .Include(c => c.Competitor)
                    .ThenInclude(c => c.Team)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetRandomNumberofCompetitors(int eventId, int number)
        {
            var competitorsInEvent = context.CompetitorsInEvent
                .Include(c => c.Competitor)
                    .ThenInclude(c => c.Team)
                .Include(c => c.Competitor)
                    .ThenInclude(c => c.Country)
                .Where(c => c.EventId.Equals(eventId)).Select(c => c);
            var randomCompetitorsList = new List<CompetitorsInEvent>();
            randomCompetitorsList = GetRandomElements(competitorsInEvent, number);
            return randomCompetitorsList; 
           
        }

        public static List<t> GetRandomElements<t>(IEnumerable<t> list, int elementsCount)
        {
            return list.OrderBy(x => Guid.NewGuid()).Take(elementsCount).ToList();
        }

        public async Task<CompetitorsInEvent> GetCompetitorsInEventByIds(int eventId, int competitorId)
        {
            var competitorsInEvent = await context.CompetitorsInEvent
                .Include(c => c.Competitor)
                    .ThenInclude(c => c.Team)
                .Include(c => c.Competitor)
                    .ThenInclude(c => c.Country)
                .Where(e => e.EventId.Equals(eventId) && e.CompetitorId.Equals(competitorId))
                .FirstOrDefaultAsync();
            return competitorsInEvent;
        }
    }
}
