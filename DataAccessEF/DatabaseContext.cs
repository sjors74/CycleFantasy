using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Drawing;

namespace Domain.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }



        public DbSet<Event> Events { get; set; }
        public DbSet<Competitor> Competitors { get; set;}
        public DbSet<Team> Teams { get; set;}
        public DbSet<CompetitorsInEvent> CompetitorsInEvent { get; set; }
        public DbSet<Country> Country { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>()
                .HasMany(c => c.Competitors)
                .WithOne(t => t.Team)
                .IsRequired();
            modelBuilder.Entity<Team>()
                .HasOne(c => c.Country);
            modelBuilder.Entity<Competitor>()
                .HasOne(c => c.Country);    
        }
    }

}
