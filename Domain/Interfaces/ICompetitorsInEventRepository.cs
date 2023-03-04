using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorsInEventRepository : IGenericRepository<CompetitorsInEvent>
    {
        Task<IEnumerable<Competitor>> GetCompetitors(int eventId);

        IEnumerable<Competitor> GetRandomNumberofCompetitors(int eventId, int number);

        Task<CompetitorsInEvent> GetCompetitorsInEventByIds(int eventId, int competitorId);
    }
}
