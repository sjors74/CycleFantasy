using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorsInEventRepository : IGenericRepository<CompetitorsInEvent>
    {
        Task<IEnumerable<CompetitorsInEvent>> GetCompetitors(int eventId);

        Task<IEnumerable<CompetitorsInEvent>> GetRandomNumberofCompetitors(int eventId, int number);

        Task<CompetitorsInEvent> GetCompetitorsInEventByIds(int eventId, int competitorId);

        Task<CompetitorsInEvent> GetById(int competitorId);
        Task<List<CompetitorsInEvent>> GetCompetitorsInEventList(int eventId);
    }
}
