using CycleManager.Domain.Dto;
using CycleManager.Domain.ViewModel;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<IEnumerable<Event>> GetActiveEvents();
        IQueryable<Event> GetAllEvents();
        Task<Event> GetEventById(int id);
        Task<EventDetailsViewModel?> GetEventDetailsViewModelById(int eventId);
        Task<IEnumerable<TeamDto>> GetTeamsForEvent(int eventId);
        Task<int> GetAantalDeelnemers(int eventId);
        Task RemoveAllTeamsFromEvent(int eventId);
        Task AddTeamToEvent(int eventId, int teamId);
        Task RemoveTeamFromEvent(int eventId, int teamId);
    }
}
