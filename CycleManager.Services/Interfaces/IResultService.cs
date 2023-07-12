using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleManager.Services.Interfaces
{
    public interface IResultService
    {
        /// <summary>
        /// Get number of results for a given stage
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        Task<int> GetResultsByStageId(int stageId);
    }
}
