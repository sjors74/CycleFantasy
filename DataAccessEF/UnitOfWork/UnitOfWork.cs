using DataAccessEF.TypeRepository;
using Domain.Context;
using Domain.Interfaces;
using Domain.Models;

namespace DataAccessEF.UnitOfWork
{
    public class UnitOfWork : IDisposable
    {
        private readonly ApplicationDbContext context;
        private GenericRepository<Competitor> competitorRepository = null!;
        private GenericRepository<CompetitorsInEvent> competitorsInEventRepository = null!;
        private GenericRepository<Event> eventRepository = null!;
        private GenericRepository<Team> teamRepository = null!;

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
        }

        public GenericRepository<Competitor> CompetitorRepository
        {
            get
            {
                if(this.competitorRepository == null)
                {
                    this.competitorRepository = new GenericRepository<Competitor>(context);
                }
                return this.competitorRepository;
            }
        }

        public GenericRepository<CompetitorsInEvent> CompetitorsInEventRepository
        {
            get
            {
                if(this.competitorsInEventRepository == null)
                {
                    this.competitorsInEventRepository = new GenericRepository<CompetitorsInEvent>(context);
                }
                return this.competitorsInEventRepository;
            }
        }
        public GenericRepository<Event> EventRepository
        {
            get
            {
                if(eventRepository == null)
                {
                    eventRepository = new GenericRepository<Event>(context);
                }
                return eventRepository;
            }
        }

        public GenericRepository<Team> Team
        {
            get
            {
                if(this.teamRepository == null)
                {
                    this.teamRepository = new GenericRepository<Team>(context);
                }
                return this.teamRepository;
            }
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
