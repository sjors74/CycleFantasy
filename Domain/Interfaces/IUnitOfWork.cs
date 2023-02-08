namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICompetitorRepository Competitor { get; }
        ICompetitorsInEventRepository CompetitorsInEvent  { get; }
        IEventRepository Event { get; }
        ITeamRepository Team { get; }
        int Save();
    }
}
