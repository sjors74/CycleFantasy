using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;

namespace DataAccessEF.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private DatabaseContext context;
        public UnitOfWork(DatabaseContext context)
        {
            this.context = context;
            Competitor = new CompetitorRepository(this.context);
            CompetitorsInEvent = new CompetitorsInEventRepository(this.context);
            Event = new EventRepository(this.context);
            Team = new TeamRepository(this.context);
        }

        public ICompetitorRepository Competitor
        {
            get; private set;
        }

        public ICompetitorsInEventRepository CompetitorsInEvent
        {
            get; private set;
        }
        public IEventRepository Event
        {
            get; private set;
        }

        public ITeamRepository Team
        {
            get; private set;
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public int Save()
        {
            return context.SaveChanges();
        }
    }
}
