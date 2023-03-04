﻿using Domain.Dto;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IResultsRepository : IGenericRepository<Result>
    {
        Task<IEnumerable<Result>> GetResultsByEventId(int eventId);

        Task<int> GetResultsByStageId(int stageId);
    }
}
