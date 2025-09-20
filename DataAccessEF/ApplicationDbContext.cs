using CycleManager.Domain.Models;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Domain.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) 
        { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Competitor> Competitors { get; set;}
        public DbSet<Team> Teams { get; set;}
        public DbSet<CompetitorsInEvent> CompetitorsInEvent { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<ConfigurationItem> ConfigurationItems { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<GameCompetitorEvent> GameCompetitorsEvent { get; set; }
        public DbSet<GameCompetitorEventPick> GameCompetitorEventPicks { get; set; }
        public DbSet<EventTeam> EventTeam { get; set; }
        public DbSet<ScrapedStageResult> ScrapedStageResults { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<DeelnemerScore> DeelnemerScores { get; set; }
        public DbSet<DeelnemerPickScore> DeelnemerPickScores { get; set; }
        public DbSet<ScrapedCompetitor> ScrapedCompetitors { get; set; }
        public DbSet<CompetitorInTeam> CompetitorInTeams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Competitor>()
                .HasOne(c => c.Country)
                .WithMany()
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stage>()
                .HasOne(e => e.Event)
                .WithMany(s => s.Stages)
                .HasForeignKey(e => e.EventId);
            modelBuilder.Entity<Stage>()
                .HasMany(s => s.Results)
                .WithOne(s => s.Stage)
                .HasForeignKey(r => r.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Stages)
                .WithOne(s => s.Event)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.GameCompetitorEvents)
                .WithOne(s => s.Event)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Configuration);
            modelBuilder.Entity<Configuration>().HasMany(c => c.ConfigurationItems);

            modelBuilder.Entity<CompetitorsInEvent>()
                .HasOne(c => c.Event)
                .WithMany(e => e.CompetitorsInEvent)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameCompetitorEvent>()
                .HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GameCompetitorEvent>()
                .HasOne(gce => gce.Event)
                .WithMany(e => e.GameCompetitorEvents)
                .HasForeignKey(gce => gce.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameCompetitorEvent>()
                .HasMany(gce => gce.Renners)
                .WithOne(p => p.GameCompetitorEvent)
                .HasForeignKey(p => p.GameCompetitorEventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameCompetitorEventPick>()
              .HasOne(p => p.CompetitorsInEvent)
              .WithMany(c => c.GameCompetitorEventPicks)
              .HasForeignKey(p => p.CompetitorsInEventId)
              .OnDelete(DeleteBehavior.Cascade); // of .Restrict als je het liever handmatig doet

            modelBuilder.Entity<GameCompetitorEventPick>()
                .HasOne(p => p.GameCompetitorEvent)
                .WithMany(gce => gce.Renners)
                .HasForeignKey(p => p.GameCompetitorEventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventTeam>()
                .HasKey(et => new { et.EventId, et.TeamId });

            modelBuilder.Entity<DeelnemerScore>()
                .HasOne(ds => ds.Stage)
                .WithMany()
                .HasForeignKey(ds => ds.StageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeelnemerPickScore>()
                .HasOne(dps => dps.Stage)
                .WithMany()
                .HasForeignKey(dps => dps.StageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompetitorInTeam>()
                .HasOne(cit => cit.Competitor)
                .WithMany(c => c.CompetitorInTeams)
                .HasForeignKey(cit => cit.CompetitorId);

            modelBuilder.Entity<CompetitorInTeam>()
                .HasOne(cit => cit.Team)
                .WithMany(t => t.CompetitorInTeams)
                .HasForeignKey(cit => cit.TeamId);


            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.Property(t => t.Name).HasMaxLength(450);
                b.Property(t => t.LoginProvider).HasMaxLength(450);
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.Property(t => t.LoginProvider).HasMaxLength(450);
                b.Property(t => t.ProviderKey).HasMaxLength(450);
            });
        }
    }
}
