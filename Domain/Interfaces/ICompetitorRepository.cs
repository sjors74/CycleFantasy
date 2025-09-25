using CycleManager.Domain.Dto;
using CycleManager.Domain.Models;
using Domain.Dto;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface ICompetitorRepository : IGenericRepository<Competitor> 
    {
        Task<List<CompetitorDto>> GetAllCompetitors(int year);
        Task<Competitor> GetById(int competitorId);
        Task<IEnumerable<CompetitorInTeamDto>> GetByTeamId(int teamId);
        Task<int> GetCompetitorsByCountry(int countryId);
        Task<List<int>> GetAvailableYears();

        Task<Competitor?> GetCompetitorByName(string firstName, string lastName, int countryId);
        IQueryable<Competitor> GetCompetitorsByTerm(string term);
        Task UpdateCompetitorWithTeam(CompetitorEditDto dto);
        Task<Competitor?> GetByIdWithTeamsAsync(int id);
        Task UpdateCompetitorAsync(Competitor competitor);
    }
}
