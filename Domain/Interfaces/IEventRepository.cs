using CycleManager.Domain.ViewModel;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<IEnumerable<Event>> GetActiveEvents();
        Task<IEnumerable<Event>> GetAllEvents();
        Task<Event> GetEventById(int id);
        Task<EventDetailsViewModel?> GetEventDetailsViewModelById(int eventId);
    }
}
