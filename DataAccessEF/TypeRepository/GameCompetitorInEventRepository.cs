using CycleManager.Domain.Interfaces;
using Domain.Context;
using Domain.Models;

namespace DataAccessEF.TypeRepository
{
    public class GameCompetitorInEventRepository : GenericRepository<GameCompetitorEvent>, IGameCompetitorInEventRepository
    {
        public GameCompetitorInEventRepository(DatabaseContext context) : base(context) 
        {
        }

        public Task<IEnumerable<GameCompetitorEvent>> GetAllGameCompetitorsInEventByEventId(int eventId)
        {
            throw new NotImplementedException();
        }
    }
}
