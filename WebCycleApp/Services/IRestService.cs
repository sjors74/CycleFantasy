using WebCycleApp.Models;

namespace WebCycleApp.Services
{
    public interface IRestService
    {
        Task<Event> GetEventByEventId(int id);
        Task<List<Competitor>> GetRandomCompetitorListByEventId(int id, int number);
    }
}
