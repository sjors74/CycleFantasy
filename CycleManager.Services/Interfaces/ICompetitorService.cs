using Domain.Models;

namespace CycleManager.Services.Interfaces
{
    public interface ICompetitorService
    {
        /// <summary>
        /// Get the number of competitors by country id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<int> GetCompetitorsByCountry(int id);
    }
}
