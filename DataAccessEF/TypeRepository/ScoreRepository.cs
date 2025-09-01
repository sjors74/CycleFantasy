using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ScoreRepository : GenericRepository<DeelnemerScore>, IScoreRepository
    {
        public ScoreRepository(ApplicationDbContext context) : base(context)
        {

        }
        public async Task<List<DeelnemerScore>> GetScoresByEventIdAsync(int eventId)
        {
            return await context.DeelnemerScores
                .Where(s => s.Stage.EventId == eventId)
                .ToListAsync();
        }
    }
}
