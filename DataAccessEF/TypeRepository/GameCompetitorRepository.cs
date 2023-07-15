using CycleManager.Domain.Interfaces;
using Domain.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class GameCompetitorRepository : GenericRepository<GameCompetitor>, IGameCompetitorRepository
    {
        public GameCompetitorRepository(DatabaseContext context) : base(context) 
        {
 
        }

        /// <summary>
        /// Get all gamecompetttors for an event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<IEnumerable<GameCompetitor>> GetAllGameCompetitorsInEvent(int eventId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<GameCompetitor>> GetAllGameCompetitorsInEvent()
        {
            return await context.GameCompetitors.ToListAsync();
        }
    }
}
