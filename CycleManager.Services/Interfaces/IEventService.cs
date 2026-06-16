using CycleManager.Domain.Dto;
using CycleManager.Domain.ViewModel;
using Domain.Dto;
using Domain.Models;

namespace CycleManager.Services
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEvents();

        Task<Event> GetEventById(int id);

        /// <summary>
        /// Add and save a new event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Create (Event entity);

        /// <summary>
        /// Update and save an event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Update(Event entity);

        /// <summary>
        /// Remove and save an event
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Delete(Event entity);

        /// <summary>
        /// Get a list of all stages for an event, return the stage number and a boolean
        /// indicating whether or not results have been recorded.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IEnumerable<StageResultDto>> GetStagesWithResultsForEvent(int eventId);
        /// <summary>
        /// Get a list of active events for a particular user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<EventForUserDto>> GetEventsByUserId(string userId);
        Task<EventDetailsViewModel?> GetEventDetailsViewModelById(int eventId);
        Task<IEnumerable<TeamDto>> GetTeamsForEvent(int eventId);
        Task<DeelnemerDto> CreatePoolAsync(DeelnemerDto deelnemerDto);
        Task SaveSelectie(SelectieDto selectie);
        Task DeletePoolAsync(int id);
        /// <summary>
        /// Return number of participants to an event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<int> GetAantalDeelnemers(int id);

        Task<IEnumerable<Event>> GetActiveEvents();

        Task RemoveAllTeamsForEvent(int eventId);

        Task AddTeamToEvent(int eventId, int teamId);

        Task RemoveTeamFromEvent(int eventId, int teamId);

        Task<RenamePoolDto> RenamePoolAsync(RenamePoolDto renamePoolDto);
    }
}
