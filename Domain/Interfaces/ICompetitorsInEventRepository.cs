using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorsInEventRepository : IGenericRepository<CompetitorsInEvent>
    {
        Task<IEnumerable<Competitor>> GetCompetitors(int eventId);

        Task<IEnumerable<Competitor>> GetRandomNumberofCompetitors(int eventId, int number);
    }
}
