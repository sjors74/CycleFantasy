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
                .Include(c => c.CompetitorInTeam)
                    .ThenInclude(c => c.Competitor) 
                        .ThenInclude(c => c.Country)
                    .Include(c => c.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                .FirstOrDefaultAsync(cie => cie.Id == id); 
            return competitorInEvent;
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetCompetitors(int eventId)
        {
            return await context.CompetitorsInEvent
                .Where(cie => cie.EventId == eventId)
                    .Include(c => c.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                    .Include(c => c.CompetitorInTeam)
                        .ThenInclude(cic => cic.Competitor)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetRandomNumberofCompetitors(int eventId, int number)
        {
            var competitorsInEvent = context.CompetitorsInEvent
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(t=> t.Team)
                        .ThenInclude(c => c.Country)
                .Include(c => c.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                .Where(cie => cie.EventId == eventId)
                .Select(cie => cie);
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
                .Include(cie => cie.CompetitorInTeam)
                .ThenInclude(cie => cie.Competitor)
                    .ThenInclude(c => c.Country)
                .Include(c => c.CompetitorInTeam)
                        .ThenInclude(cit => cit.Team)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.CompetitorInTeamId == competitorId);

            return competitorsInEvent;
        }
    }
}
