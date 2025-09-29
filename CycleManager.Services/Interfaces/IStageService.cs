using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface IStageService
    {
        /// <summary>
        /// Get al ist of all stages in an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<IEnumerable<Stage>> GetStagesByEventId(int eventId);
        /// <summary>
        /// Return the stageNumber for a specific date for a specific event.
        /// Return 0 when no stage(number) is found.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<int> GetStageNumberForDateAsync(DateTime date, int eventId);

        /// <summary>
        /// Get the number of scraped results for a specific stage
        /// </summary>
        /// <param name="stageNumber"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<int> GetStageResults(int stageNumber, int eventId);

        /// <summary>
        /// Get the stageId for an event and stagenumber
        /// </summary>
        /// <param name="stageNumber"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<int> GetStageIdFromStageNumber(int stageNumber, int eventId);
        /// <summary>
        /// Get the stage for an event and stagenumber
        /// </summary>
        /// <param name="stageNumber"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        Task<Stage> GetStage(int stageNumber, int eventId);

        Task AddStage(Stage stage);

        Task<bool> DeleteStage(int id);

        Task<Stage> GetStageById(int id);

        Task UpdateStage(Stage stage);
    }
}
