using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorsInEventRepository : IGenericRepository<Competitor>
    {
        Task<IEnumerable<Competitor>> GetCompetitors(int eventId);

        IEnumerable<Competitor> GetRandomNumberofCompetitors(int eventId, int number);
    }
}
