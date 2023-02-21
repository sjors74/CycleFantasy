using Domain.Context;
using Domain.Dto;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.TypeRepository
{
    public class ResultsRepository : GenericRepository<ResultDto>, IResultsRepository
    {
        public ResultsRepository(DatabaseContext context) : base(context)
        {

        }

        public async Task<IEnumerable<ResultDto>> GetResultsByStageId(int stageId)
        {
            var results = new List<ResultDto>();
            var resultsDb = await context.Results
                .Include(c => c.CompetitorInEvent.Competitor)
                .Where(r => r.StageId == stageId)
                .OrderBy(r => r.ConfigurationItem.Position).ToListAsync();

            foreach(var resultItem in resultsDb)
            {
                results.Add(
                    new ResultDto
                    {
                        CompetitorName = resultItem.CompetitorInEvent.Competitor.CompetitorName,
                        Points = resultItem.ConfigurationItem.Score,
                        Position = resultItem.ConfigurationItem.Position,
                        StageNumber = resultItem.Stage.StageName
                    });
            }

            return results;
        }
    }
}
