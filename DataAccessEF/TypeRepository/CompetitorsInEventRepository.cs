using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorsInEventRepository: GenericRepository<CompetitorsInEvent>, ICompetitorsInEventRepository
    {
        public CompetitorsInEventRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Competitor>> GetCompetitors(int eventId)
        {
            var competitorsInEvent = context.CompetitorsInEvent.Where(c => c.EventId.Equals(eventId)).Select(c => c.CompetitorId);
            var competitors = context.Competitors.Where(c => competitorsInEvent.Contains(c.CompetitorId));
            return await competitors.ToListAsync();
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetRandomNumberofCompetitors(int eventId, int number)
        {
            var competitorsInEvent = context.CompetitorsInEvent.Where(c => c.EventId.Equals(eventId)).Select(c => c);
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
            return await context.CompetitorsInEvent.Where(e => e.EventId.Equals(eventId) && e.CompetitorId.Equals(competitorId)).FirstOrDefaultAsync();

        }
    }
}
