using Domain.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class StageRepository : GenericRepository<Stage>, IStageRepository
    {
        public StageRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Stage>> GetByEventId(int eventId)
        {
            var stages = await context.Stages
                .Include(e => e.Event)
                .Where(s => s.EventId.Equals(eventId))
                .OrderBy(s => s.StageOrder)
                .ToListAsync();
            return stages;
        }

        public async Task<int> GetStageId(int stageNumber, int eventId)
        {
            var stageNumberString = stageNumber.ToString();
            var stage = await context.Stages
                .Include(e => e.Event)
                .Where(s => s.EventId == eventId && s.StageName == stageNumberString)
                .FirstOrDefaultAsync();

            if (stage != null)
            {
                return stage.Id;
            }
            return 0;
        }

        public async Task<int> GetStageNumber(DateTime date, int eventId)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            var stage = await context.Stages
                .Where(s => s.EventId.Equals(eventId) && s.StageDate.Date >= startDate && s.StageDate < endDate)
                .FirstOrDefaultAsync();
            if (stage != null && int.TryParse(stage.StageName, out int stageNumber))
            {
                return stageNumber;
            }
            else
            {
                return 0;
            }
        }

        public async Task<int> GetStagesResults(int stageNumber, int eventId)
        {
            return await context.ScrapedStageResults
                .Where(s => s.StageId.Equals(stageNumber) && s.EventId.Equals(eventId))
                .CountAsync();
        }

        public async Task<Stage> GetStage(int stageNumber, int eventId)
        {
            var stage = await context.Stages
                .Where(s => s.EventId.Equals(eventId) && s.StageName.Equals(stageNumber.ToString()))
                .FirstOrDefaultAsync();
            if (stage != null)
            {
                return stage;
            }
            else
            {
                return new Stage();
            }
        }

        public async Task<Stage> GetStageById(int stageId)
        {
            var stage = await context.Stages
                .Include(s => s.Event)
                .FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage != null)
            {
                return stage;
            }
            else
            {
                return new Stage();
            }
        }
    }
}
