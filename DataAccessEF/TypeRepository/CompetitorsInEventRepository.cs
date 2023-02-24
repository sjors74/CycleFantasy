using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorsInEventRepository: GenericRepository<Competitor>, ICompetitorsInEventRepository
    {
        public CompetitorsInEventRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Competitor>> GetCompetitors(int eventId)
        {
            var competitorsInEvent = context.CompetitorsInEvent.Include(c => c.Competitor).Where(c => c.EventId.Equals(eventId)).Select(c => c.Competitor);
            return await competitorsInEvent.ToListAsync();
        }

        public async Task<IEnumerable<Competitor>> GetRandomNumberofCompetitors(int eventId, int number)
        {
            var competitorsInEvent = context.CompetitorsInEvent.Include(c => c.Competitor).Where(c => c.EventId.Equals(eventId)).Select(c => c.Competitor);
            var randomCompetitorsList = new List<Competitor>();
            randomCompetitorsList = GetRandomElements(competitorsInEvent, number);
            return randomCompetitorsList; 
           
        }

        public static List<t> GetRandomElements<t>(IEnumerable<t> list, int elementsCount)
        {
            return list.OrderBy(x => Guid.NewGuid()).Take(elementsCount).ToList();
        }
    }
}
