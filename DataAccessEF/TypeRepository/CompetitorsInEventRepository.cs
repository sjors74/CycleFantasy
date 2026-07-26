using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class CompetitorsInEventRepository: GenericRepository<CompetitorsInEvent>, ICompetitorsInEventRepository
    {
        private readonly Func<IEnumerable<CompetitorsInEvent>, IEnumerable<CompetitorsInEvent>> _randomizer;
        public CompetitorsInEventRepository(ApplicationDbContext context, 
            Func<IEnumerable<CompetitorsInEvent>, IEnumerable<CompetitorsInEvent>>? randomizer = null) : base(context)
        {
            _randomizer = randomizer ?? (list => list.OrderBy(x => Guid.NewGuid()));
        }

        public async Task<CompetitorsInEvent> GetById(int id)
        {
            var competitorInEvent = await context.CompetitorsInEvent
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.Competitor)
                        .ThenInclude(c => c.Country)
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.TeamYear)
                        .ThenInclude(ty => ty.Team)
                .FirstOrDefaultAsync(cie => cie.Id == id);

            return competitorInEvent;
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetCompetitors(int eventId)
        {
            return await context.CompetitorsInEvent
                .Where(cie => cie.EventId == eventId)
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.TeamYear)
                        .ThenInclude(ty => ty.Team)
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.Competitor)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompetitorsInEvent>> GetRandomNumberofCompetitors(int eventId, int number)
        {
            var competitorsInEvent = await context.CompetitorsInEvent
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.Competitor)
                        .ThenInclude(c => c.Country)
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.TeamYear)
                        .ThenInclude(ty => ty.Team)
                .Where(cie => cie.EventId == eventId)
                .ToListAsync();

            var randomCompetitorsList = _randomizer(competitorsInEvent)
                .Take(number)
                .ToList();

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
                    .ThenInclude(cit => cit.Competitor)
                        .ThenInclude(c => c.Country)
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.TeamYear)
                        .ThenInclude(ty => ty.Team)
                .FirstOrDefaultAsync(e =>
                    e.EventId == eventId &&
                    e.CompetitorInTeamId == competitorId);

            return competitorsInEvent;
        }

        public async Task<List<CompetitorsInEvent>> GetCompetitorsInEventList(int eventId)
        {
            return await context.CompetitorsInEvent
                .Include(cie => cie.CompetitorInTeam)
                    .ThenInclude(cit => cit.Competitor)
                        .ThenInclude(c => c.CompetitorInTeams)
                            .ThenInclude(cit => cit.TeamYear)
                                .ThenInclude(ty => ty.Team)
                .Where(cie => cie.EventId == eventId && !cie.OutOfCompetition)
                .OrderBy(cie => cie.CompetitorInTeam.Competitor.FirstName)
                .ToListAsync();
        }
    }
}
